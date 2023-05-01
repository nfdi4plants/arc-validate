module CvTokenHelperFunctions

open ArcGraphModel
open ArcGraphModel.IO
open ArcGraphModel.ArcType


module CvParam =

    /// Returns a CvParam's cell address (in the form of `<sheetname>!<column letter><row number>`, e.g. `sheet1!A1`) for a given column
    /// type if `predicate` returns true. Else returns None.
    let returnCellAddressWhen predicate columnType (cvParam : CvParam) =
        match BuildingBlockType.tryOfString (CvBase.getCvName cvParam)with
        | Some x when columnType x ->
            let isPredicate = 
                Param.getValueAsString cvParam
                |> predicate
            if isPredicate then CvParam.tryGetAttribute "Path" cvParam |> Option.map Param.getValueAsString
            else None
        | _ -> None

    /// Returns true if a given CvParam contains a Filepath as its ParamValue.
    let containsFilePath cvParam =
        let buildingBlockCondition = fun c -> c = RawDataFile || c = DerivedDataFile || c = ProtocolREF
        returnCellAddressWhen String.isFilepath buildingBlockCondition cvParam

    //let returnIncorrectFilePathCells cvParams =
    //    cvParams |> List.choose (returnCellAddressWhen ())

    /// Returns all Source Name cells with an empty value.
    let returnEmptyInputCells cvParams = 
        cvParams |> List.choose (returnCellAddressWhen String.isNullOrWhiteSpace (fun c -> c.IsInputColumn))

    /// Returns all Sample Name / Raw Data File / Derived Data File cells with an empty value.
    let returnEmptyOutputCells cvParams = 
        cvParams |> List.choose (returnCellAddressWhen String.isNullOrWhiteSpace (fun c -> c.IsOutputColumn))

    /// Returns all RawDataFile names, DerivedDataFile names and ProtocolREF filenames that do not exist in the ARC.
    let returnNonexistentFileNames cvParams =
        // root path to ARC folder must be part of the string at this point already
        cvParams |> List.choose (returnCellAddressWhen (System.IO.File.Exists >> not) (fun c -> c = RawDataFile || c = DerivedDataFile || c = ProtocolREF))


module CvContainer =

    /// Returns a given property name's value as string if it exists. Else returns None.
    let tryGetPropertyStringValue property cvContainer =
        CvContainer.tryGetSingle property cvContainer 
        |> Option.map (Param.tryParam >> Option.map Param.getValueAsString) 
        |> Option.flatten

    /// Returns the value of the first attribute with the given name in the properties of a given CvContainer.
    let getAttributeValueFromProperties attribute (cvContainer : CvContainer) =
        cvContainer.Properties.Values 
        |> Seq.concat 
        |> Seq.toList 
        |> List.choose CvBase.tryAs<CvParam>
        |> List.pick (fun c ->
            // you can use `CvTerm.getName attribute` if you want `attribute` to be of `CvTerm`
            CvAttributeCollection.tryGetAttribute attribute c
            |> Option.map Param.getValue
        )

    /// Returns the value as string of the first attribute with the given name in the properties of a given CvContainer.
    let getAttributeValueAsStringFromProperties attribute cvContainer =
        getAttributeValueFromProperties attribute cvContainer |> string

    /// Returns the value as int of the first attribute with the given name in the properties of a given CvContainer.
    let getAttributeValueAsIntFromProperties attribute cvContainer =
        getAttributeValueFromProperties attribute cvContainer :?> int

    /// Checks if a given CvContainer has an Investigation CvTerm.
    let isPartOfInvestigation (cvc : CvContainer) =
        cvc.Properties.Values
        |> Seq.exists (
            Seq.exists (
                CvParam.tryCvParam
                >> Option.get
                >> fun cvp -> cvp.Attributes
                >> List.exists (
                    fun ip -> CvBase.getCvName ip = CvTerm.getName Terms.investigation
                )
            )
        )
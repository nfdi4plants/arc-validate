module ARCGraphTests


open ARCExpect
open Expecto
open ARCTokenization
open ControlledVocabulary


let mockInv = 
    ARCMock.InvestigationMetadataTokens(
        Investigation_Identifier = ["iid"],
        Investigation_Title = ["ititle"],
        Investigation_Description = ["idesc"],
        Investigation_Person_Last_Name = ["Maus"; "Keider"; "müller"; ""; "oih"],
        Investigation_Person_First_Name = ["Oliver"; ""; "andreas";],
        Investigation_Person_Mid_Initials = ["L. I."; "C."],
        Investigation_Person_Email = ["maus@nfdi4plants.org"],
        Investigation_Person_Affiliation = [""; "Affe"],
        Study_Identifier = ["sid"],
        Study_Title = ["stitle"],
        Study_Description = ["sdesc"],
        Study_File_Name = [@"sid\isa.study.xlsx"],
        Study_Assay_File_Name = [@"aid\isa.assay.xlsx"; @"aid2\isa.assay.xlsx"],
        Study_Person_Last_Name = ["weil"],
        Study_Person_First_Name = [""; "lukas"]
    )
    |> List.concat // use flat list
    |> Seq.map (fun cvp -> cvp :> IParam)


[<Tests>]
let ``fillTokenList tests`` =
    testCase "returns correct output seq" (fun _ ->
        let actual = ARCGraph.fillTokenList Terms.InvestigationMetadata.ontology mockInv
        let act1 = actual |> Seq.tryItem 1 |> Option.bind (Seq.tryFind (fun t -> t |> Seq.exists (fun (t1,t2) -> t1 = ("Investigation Identifier", 1))))
        Expect.isSome act1 "missing Investigation Identifier"
        let act2 = 
            Option.defaultValue Seq.empty act1 
            |> Seq.map (fun (t1,t2) -> t2.Value |> ParamValue.getValueAsString) 
            |> Seq.tryHead 
            |> Option.defaultValue ""
        let exp2 = "iid"
        Expect.equal act2 exp2 "wrong Investigation Identifier"

        let act3 = actual |> Seq.tryItem 1 |> Option.bind (Seq.tryFind (fun t -> t |> Seq.exists (fun (t1,t2) -> t1 = ("Investigation Title", 1))))
        Expect.isSome act3 "missing Investigation Title"
        let act4 = 
            Option.defaultValue Seq.empty act3 
            |> Seq.map (fun (t1,t2) -> t2.Value |> ParamValue.getValueAsString) 
            |> Seq.tryItem 2
            |> Option.defaultValue ""
        let exp4 = "ititle"
        Expect.equal act4 exp4 "wrong Investigation Title"

        let act5 = actual |> Seq.tryItem 1 |> Option.bind (Seq.tryFind (fun t -> t |> Seq.exists (fun (t1,t2) -> t1 = ("Investigation Description", 1))))
        Expect.isSome act5 "missing Investigation Description"
        let act6 = 
            Option.defaultValue Seq.empty act5
            |> Seq.map (fun (t1,t2) -> t2.Value |> ParamValue.getValueAsString) 
            |> Seq.tryItem 3
            |> Option.defaultValue ""
        let exp6 = "idesc"
        Expect.equal act6 exp6 "wrong Investigation Description"

        let act7 = actual |> Seq.tryItem 3 |> Option.bind (Seq.tryFind (fun t -> t |> Seq.exists (fun (t1,t2) -> t1 = ("Investigation Person Last Name", 5))))
        Expect.isSome act7 "missing Investigation Person Last Name"
        let act8 = 
            Option.defaultValue Seq.empty act7
            |> Seq.map (fun (t1,t2) -> t2.Value |> ParamValue.getValueAsString) 
            |> Seq.tryItem 0
            |> Option.defaultValue ""
        let exp8 = "oih"
        Expect.equal act8 exp8 "wrong Investigation Person Last Name"
    )
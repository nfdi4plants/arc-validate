module ARCGraphTests


open ARCExpect
open Expecto
open ARCTokenization
open ControlledVocabulary


let mockInv = 
    let userComment1 = CvParam(Terms.StructuralTerms.userComment, CvValue Terms.StructuralTerms.metadataSectionKey) :> IParam
    let userComment2 = CvParam(Terms.StructuralTerms.userComment, Value "Keywords") :> IParam
    ARCMock.InvestigationMetadataTokens(
        false,
        Investigation_Identifier = ["ArcPrototype"],
        Investigation_Title = ["ArcPrototype"],
        Investigation_Description = ["A prototypic ARC that implements all specification standards accordingly"],
        Investigation_Person_Last_Name = ["Mühlhaus"; "Garth"; "Maus"],
        Investigation_Person_First_Name = ["Timo"; "Christoph"; "Oliver";],
        Investigation_Person_Email = ["timo.muehlhaus@rptu.de"; "garth@rptu.de"; "maus@nfdi4plants.org"],
        Investigation_Person_Affiliation = ["RPTU University of Kaiserslautern"; "RPTU University of Kaiserslautern"; "RPTU University of Kaiserslautern"],
        Investigation_Person_Phone = ["0 49 (0)631 205 4657"],
        Investigation_Person_Address = ["RPTU University of Kaiserslautern, Paul-Ehrlich-Str. 23 , 67663 Kaiserslautern"; ""; "RPTU University of Kaiserslautern, Erwin-Schrödinger-Str. 56 , 67663 Kaiserslautern"],
        Investigation_Person_Roles = ["principal investigator"; "principal investigator"; "research assistant"],
        Investigation_Person_Roles_Term_Source_REF = ["scoro"; "scoro"; "scoro"],
        Investigation_Person_Roles_Term_Accession_Number = ["http://purl.org/spar/scoro/principal-investigator"; "http://purl.org/spar/scoro/principal-investigator"; "http://purl.org/spar/scoro/research-assistant"],
        Comment_ORCID = ["http://orcid.org/0000-0003-3925-6778"; ""; "0000-0002-8241-5300"],
        Study_Identifier = ["experiment1_material"; "experiment2"],
        Study_File_Name = ["experiment1_material/isa.study.xlsx"; "experiment2/isa.study.xlsx"],
        Study_Assay_File_Name = ["measurement1/isa.assay.xlsx"; "measurement2/isa.assay.xlsx"]
    )
    |> List.concat // use flat list
    |> Seq.map (fun cvp -> cvp :> IParam)
    |> Seq.insertAt 12 userComment1
    |> Seq.insertAt 13 userComment2

let mockStu =
    ARCMock.StudyMetadataTokens(
        false,
        Study_Identifier = ["experiment1_material"],
        Study_Title = ["Prototype for experimental data"],
        Study_Description = ["In this a devised study to have an exemplary experimental material description."],
        Study_File_Name = ["experiment1_material/isa.study.xlsx"]
    )
    |> List.concat // use flat list
    |> Seq.map (fun cvp -> cvp :> IParam)

let mockAss =
    ARCMock.AssayMetadataTokens(
        false,
        Assay_File_Name = ["measurement1/isa.assay.xlsx"],
        Assay_Person_Last_Name = ["Maus"; "Katz"],
        Assay_Person_First_Name = ["Oliver"; "Marius"],
        Assay_Person_Mid_Initials = [""; "G."],
        Assay_Person_Email = ["maus@nfdi4plants.org"],
        Assay_Person_Affiliation = ["RPTU University of Kaiserslautern"],
        Assay_Person_Roles = ["research assistant"],
        Assay_Person_Roles_Term_Accession_Number = ["http://purl.org/spar/scoro/research-assistant"],
        Assay_Person_Roles_Term_Source_REF = ["scoro"]
    )
    |> List.concat // use flat list
    |> Seq.map (fun cvp -> cvp :> IParam)


[<Tests>]
let ``fillTokenList tests`` =
    testList "returns correct output seq" [
        testCase "Investigation" (fun _ ->
            let actual = ARCGraph.fillTokenList Terms.InvestigationMetadata.ontology mockInv
            let act1 = actual |> Seq.tryItem 1 |> Option.bind (Seq.tryFind (fun t -> t |> Seq.exists (fun (t1,t2) -> t1 = ("Investigation Identifier", 1))))
            Expect.isSome act1 "missing Investigation Identifier"
            let act2 = 
                Option.defaultValue Seq.empty act1 
                |> Seq.map (fun (t1,t2) -> t2.Value |> ParamValue.getValueAsString) 
                |> Seq.tryHead 
                |> Option.defaultValue ""
            let exp2 = "ArcPrototype"
            Expect.equal act2 exp2 "wrong Investigation Identifier"

            let act3 = actual |> Seq.tryItem 1 |> Option.bind (Seq.tryFind (fun t -> t |> Seq.exists (fun (t1,t2) -> t1 = ("Investigation Title", 1))))
            Expect.isSome act3 "missing Investigation Title"
            let act4 = 
                Option.defaultValue Seq.empty act3 
                |> Seq.map (fun (t1,t2) -> t2.Value |> ParamValue.getValueAsString) 
                |> Seq.tryItem 2
                |> Option.defaultValue ""
            let exp4 = "ArcPrototype"
            Expect.equal act4 exp4 "wrong Investigation Title"

            let act5 = actual |> Seq.tryItem 1 |> Option.bind (Seq.tryFind (fun t -> t |> Seq.exists (fun (t1,t2) -> t1 = ("Investigation Description", 1))))
            Expect.isSome act5 "missing Investigation Description"
            let act6 = 
                Option.defaultValue Seq.empty act5
                |> Seq.map (fun (t1,t2) -> t2.Value |> ParamValue.getValueAsString) 
                |> Seq.tryItem 3
                |> Option.defaultValue ""
            let exp6 = "A prototypic ARC that implements all specification standards accordingly"
            Expect.equal act6 exp6 "wrong Investigation Description"

            let act7 = actual |> Seq.tryItem 3 |> Option.bind (Seq.tryFind (fun t -> t |> Seq.exists (fun (t1,t2) -> t1 = ("Investigation Person Last Name", 2))))
            Expect.isSome act7 "missing Investigation Person Last Name"
            let act8 = 
                Option.defaultValue Seq.empty act7
                |> Seq.map (fun (t1,t2) -> t2.Value |> ParamValue.getValueAsString) 
                |> Seq.tryItem 0
                |> Option.defaultValue ""
            let exp8 = "Garth"
            Expect.equal act8 exp8 "wrong Investigation Person Last Name"

            let act9 = actual |> Seq.tryItem 4 |> Option.bind (Seq.tryFind (fun t -> t |> Seq.exists (fun (t1,t2) -> t1 = ("Study Identifier", 1))))
            Expect.isSome act9 "missing Study Identifier"
            let act10 = 
                Option.defaultValue Seq.empty act9
                |> Seq.map (fun (t1,t2) -> t2.Value |> ParamValue.getValueAsString) 
                |> Seq.tryItem 0
                |> Option.defaultValue ""
            let exp10 = "experiment1_material"
            Expect.equal act10 exp10 "wrong Study Identifier"

            let act11 = actual |> Seq.tryItem 1 |> Option.bind (Seq.tryFind (fun t -> t |> Seq.exists (fun (t1,t2) -> t1 = ("User Comment", 1))))
            Expect.isSome act11 "missing User Comment"
            let act12 = 
                Option.defaultValue Seq.empty act11
                |> Seq.map (fun (t1,t2) -> t2.Value |> ParamValue.getValueAsString) 
                |> Seq.tryItem 4
                |> Option.defaultValue ""
            let exp12 = "Keywords"
            Expect.equal act12 exp12 "wrong User Comment"
        )

        testCase "Study" (fun _ ->
            let actual = ARCGraph.fillTokenList Terms.StudyMetadata.ontology mockStu
            let act1 = actual |> Seq.tryItem 0 |> Option.bind (Seq.tryFind (fun t -> t |> Seq.exists (fun (t1,t2) -> t1 = ("Study Identifier", 1))))
            Expect.isSome act1 "missing Study Identifier"
            let act2 = 
                Option.defaultValue Seq.empty act1 
                |> Seq.map (fun (t1,t2) -> t2.Value |> ParamValue.getValueAsString) 
                |> Seq.tryItem 0
                |> Option.defaultValue ""
            let exp2 = "experiment1_material"
            Expect.equal act2 exp2 "wrong Study Identifier"
        )

        testCase "Assay" (fun _ ->
            let actual = ARCGraph.fillTokenList Terms.AssayMetadata.ontology mockAss
            let act1 = actual |> Seq.tryItem 0 |> Option.bind (Seq.tryFind (fun t -> t |> Seq.exists (fun (t1,t2) -> t1 = ("Assay File Name", 1))))
            Expect.isSome act1 "missing Assay Filename"
            let act2 = 
                Option.defaultValue Seq.empty act1 
                |> Seq.map (fun (t1,t2) -> t2.Value |> ParamValue.getValueAsString) 
                |> Seq.tryItem 8
                |> Option.defaultValue ""
            let exp2 = "measurement1/isa.assay.xlsx"
            Expect.equal act2 exp2 "wrong Assay Filename"
        )
    ]
module ARCGraphTests


open ARCExpect
open Expecto
open ARCTokenization


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



[<Tests>]
let ``fillTokenList tests`` =
    testCase "returns correct output seq" (fun _ ->
        let actual = ARCGraph.fillTokenList Terms.InvestigationMetadata.ontology mockInv
        let act1 = actual |> Seq.item 
        let exp1 = 
        Expect.equal act1 exp1 "incorrect output"
    )
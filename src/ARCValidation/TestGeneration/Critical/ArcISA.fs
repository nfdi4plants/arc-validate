namespace ArcValidation.TestGeneration.Critical.Arc

open ArcValidation
open ArcValidation.Configs

module ISA =

    open Expecto
    open FSharpAux

    let generateISATests (arcConfig : ArcConfig) =

        let pathConfig = arcConfig.PathConfig

        testList "ISA" [
            testList "Semantic" [
                testList "Investigation" [
                    testList "Studies" [
                    ]
                ]
            ]
        ]


//open Expecto
//open ParamCollection
//open IsaTerms
//open IsaTerms.Investigation

//let filePathCv = CvParam(IoTerms.FilePath,"/isa.investigation (Venni).xlsx") 

//let isaInvestigationTokens = 
//    __SOURCE_DIRECTORY__ + "/isa.investigation (Venni).xlsx"
//    |> ARCTokenization.Investigation.parseMetadataSheetFromFile
//    |> List.map (fun iparam -> 
//        match CvParam.tryAddAttribute filePathCv iparam with
//        | _ -> iparam
//        )



//let simpleTest =
//  test "A simple test" {
//    match tryFindby Contacts.``Investigation Person Email`` isaInvestigationTokens with
//    | Some cvp ->
//        ArcExpect.ByValue.notEmpty cvp
//        ArcExpect.ByValue.isMatch StringValidationPattern.email  cvp   
//    | None -> ()
//  }



 



//let emailTests=        
//    testList "Email required"         
//        [
//        for cvp in findAllby Contacts.``Investigation Person Email`` isaInvestigationTokens do            
//            ArcExpect.test TestID.Guid
//                {
                                        

//                    // ArcExpect.either 
//                    //     { ArcExpect.ByValue.notEmpty cvp }
//                    // ``or`` ArcExpect.either
//                    //     {ArcExpect.ByValue.isMatch @"^[^@\s]+@[^@\s]+\.[^@\s]+$" cvp }
                    
                     
//                    //this (ArcExpect.ByValue.notEmpty cvp)

//                    if not <| CvParamExtensions.isEmpty cvp then
//                        ArcExpect.ByValue.isMatch @"^[^@\s]+@[^@\s]+\.[^@\s]+$"  cvp   

                    
//                    // either                   
//                    //     <| fun () -> ArcExpect.ByValue.notEmpty cvp
//                    //     <| fun () -> ArcExpect.ByValue.isMatch @"^[^@\s]+@[^@\s]+\.[^@\s]+$"  cvp


//                    // ArcExpect.ByValue.notEmpty cvp
                    
//                    // try
//                    //     ArcExpect.ByValue.notEmpty cvp
//                    // with
//                    // | ex1 -> 
//                    //     try
//                    //         ArcExpect.ByValue.isMatch @"^[^@\s]+@[^@\s]+\.[^@\s]+$"  cvp   
//                    //     with
//                    //     | ex2 ->                          
//                    //         Expecto.Tests.failtestNoStackf "Probleme is either\n %s \n or\n %s" ex1.Message ex2.Message

//                }
//        ]  



//runTestsWithCLIArgs [] [||] emailTests
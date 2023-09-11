namespace ARCExpect.TestGeneration.Critical.ARC

open ARCExpect
open ARCExpect.Configs

module ISA =

    open Expecto
    open FSharpAux

    let generateISATests (arcConfig : ARCConfig) =

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
//        ARCExpect.ByValue.notEmpty cvp
//        ARCExpect.ByValue.isMatch StringValidationPattern.email  cvp   
//    | None -> ()
//  }



 



//let emailTests=        
//    testList "Email required"         
//        [
//        for cvp in findAllby Contacts.``Investigation Person Email`` isaInvestigationTokens do            
//            ARCExpect.test TestID.Guid
//                {
                                        

//                    // ARCExpect.either 
//                    //     { ARCExpect.ByValue.notEmpty cvp }
//                    // ``or`` ARCExpect.either
//                    //     {ARCExpect.ByValue.isMatch @"^[^@\s]+@[^@\s]+\.[^@\s]+$" cvp }
                    
                     
//                    //this (ARCExpect.ByValue.notEmpty cvp)

//                    if not <| CvParamExtensions.isEmpty cvp then
//                        ARCExpect.ByValue.isMatch @"^[^@\s]+@[^@\s]+\.[^@\s]+$"  cvp   

                    
//                    // either                   
//                    //     <| fun () -> ARCExpect.ByValue.notEmpty cvp
//                    //     <| fun () -> ARCExpect.ByValue.isMatch @"^[^@\s]+@[^@\s]+\.[^@\s]+$"  cvp


//                    // ARCExpect.ByValue.notEmpty cvp
                    
//                    // try
//                    //     ARCExpect.ByValue.notEmpty cvp
//                    // with
//                    // | ex1 -> 
//                    //     try
//                    //         ARCExpect.ByValue.isMatch @"^[^@\s]+@[^@\s]+\.[^@\s]+$"  cvp   
//                    //     with
//                    //     | ex2 ->                          
//                    //         Expecto.Tests.failtestNoStackf "Probleme is either\n %s \n or\n %s" ex1.Message ex2.Message

//                }
//        ]  



//runTestsWithCLIArgs [] [||] emailTests
module ARCSpecification

open Expecto
open System.IO
open ARCTokenization


[<Tests>]
let tests =
    testList "ARCValidationPackage tests" [
        test "V2_Draft" {

            let testPath = System.IO.Path.GetFullPath("fixtures/arcs/testingArc/")

            let cases = ARCValidate.ARCSpecification.V2_Draft.testCases testPath

            
            let runSummery = ARCExpect.Execute.Validation cases

            //false None (ParseResults<ARCValidate.CLIArguments.ValidateArgs>.Empty)
            Expect.isTrue
                runSummery.successful
                "ARCValidationPackage cache date was not updated correctly."
            
        }
       
    ]
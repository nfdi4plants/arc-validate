module ARCSpecification

open Expecto


[<Tests>]
let tests =
    testList "ARCValidationPackage tests" [
        test "V2_Draft" {

            let testPath: string = @"../../../../fixtures/arcs/testingArc"

            let cases = ARCExpect.ARCSpecification.V2_Draft.validationCases testPath

            let runSummary = ARCExpect.Execute.Validation cases

            //false None (ParseResults<ARCValidate.CLIArguments.ValidateArgs>.Empty)
            Expect.isTrue
                runSummary.successful
                "The validation cases for ARCSpecification.V2_Draft do not validate a structurally correct ARC."
            
        }
       
    ]
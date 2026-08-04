module CommandLineArgumentTests

open ARCValidate.CLIArguments
open Expecto

[<Tests>]
let ``package argument boundary tests`` =
    testList "package argument boundary" [
        test "splits only at the first boundary and preserves tokens exactly" {
            let hostileValue = "\"; $(touch injected) & <xml> `literal`"

            let actual =
                PackageArgumentBoundary.split [|
                    "validate"
                    "-p"
                    "example"
                    "--"
                    "--echo"
                    hostileValue
                    "--"
                    "literal-tail"
                |]

            Expect.sequenceEqual
                actual.CLIArguments
                [| "validate"; "-p"; "example" |]
                "arc-validate arguments"

            Expect.sequenceEqual
                actual.PackageArguments
                [| "--echo"; hostileValue; "--"; "literal-tail" |]
                "Package tokens must not be interpreted or normalized"

            Expect.isTrue actual.HasBoundary "Boundary was found"
        }

        test "appends package arguments after the four standard arguments" {
            let hostileValue = "a value; & $(still-literal)"

            let actual =
                PackageProcessArguments.create
                    "/arc path/"
                    "/output path"
                    (Some "feature/args")
                    (Some "abc123")
                    [| "--echo"; hostileValue |]

            Expect.sequenceEqual
                actual
                [|
                    "-i"
                    "/arc path/"
                    "-o"
                    "/output path"
                    "--source-branch"
                    "feature/args"
                    "--source-commit-hash"
                    "abc123"
                    "--echo"
                    hostileValue
                |]
                "The child receives one unchanged argument array"
        }
    ]

module CommandLineArgumentTests

open ARCValidate.CLIArguments
open ARCValidate.CLICommands
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

        test "parses the explicit config resolve command and path" {
            let parsed =
                ARCValidateCommand.createParser().ParseCommandLine [|
                    "config"
                    "resolve"
                    "--validation-config"
                    ".arc/validation_packages.yml"
                |]

            match parsed.GetSubCommand() with
            | ARCValidateCommand.Config config ->
                match config.GetSubCommand() with
                | ConfigCommand.Resolve resolve ->
                    Expect.equal
                        (resolve.GetResult(ConfigResolveArgs.Validation_Config))
                        ".arc/validation_packages.yml"
                        "Explicit validation config path"
            | command -> failtestf "Expected config resolve, got %A" command
        }
    ]

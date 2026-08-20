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

        test "accepts configured validation with an optional digest" {
            let parsed =
                ARCValidateCommand.createParser().ParseCommandLine [|
                    "validate"
                    "--package"
                    "configured"
                    "--package-version"
                    "1.2.7"
                    "--validation-config"
                    ".arc/validation_packages.yml"
                    "--validation-config-sha256"
                    (String.replicate 64 "a")
                |]

            match parsed.GetSubCommand() with
            | ARCValidateCommand.Validate validate ->
                Expect.equal
                    (ValidateArgs.validateCombination validate false)
                    (Ok())
                    "Configured mode is valid"
            | command -> failtestf "Expected validate, got %A" command
        }

        test "classifies every configured and manual selector combination" {
            let parser = ARCValidateCommand.createParser()

            for hasConfig in [ false; true ] do
                for hasDigest in [ false; true ] do
                    for hasPackage in [ false; true ] do
                        for hasVersion in [ false; true ] do
                            for hasBoundary in [ false; true ] do
                                let inputs =
                                    [
                                        "validate"

                                        if hasPackage then
                                            "--package"
                                            "configured"

                                        if hasVersion then
                                            "--package-version"
                                            "1.2.7"

                                        if hasConfig then
                                            "--validation-config"
                                            ".arc/validation_packages.yml"

                                        if hasDigest then
                                            "--validation-config-sha256"
                                            String.replicate 64 "a"
                                    ]
                                    |> List.toArray

                                let arguments =
                                    match parser.ParseCommandLine(inputs).GetSubCommand() with
                                    | ARCValidateCommand.Validate validate -> validate
                                    | command -> failtestf "Expected validate, got %A" command

                                let expectedValid =
                                    (not hasDigest || hasConfig)
                                    && (not hasConfig
                                        || (hasPackage && hasVersion && not hasBoundary))
                                    && (not hasBoundary || hasPackage)

                                let actualValid =
                                    ValidateArgs.validateCombination arguments hasBoundary
                                    |> Result.isOk

                                Expect.equal
                                    actualValid
                                    expectedValid
                                    $"config={hasConfig}, digest={hasDigest}, package={hasPackage}, version={hasVersion}, boundary={hasBoundary}"
        }

        test "rejects incomplete configured mode and the raw boundary" {
            let validate inputs =
                let parsed =
                    ARCValidateCommand.createParser().ParseCommandLine(
                        Array.append [| "validate" |] inputs
                    )

                match parsed.GetSubCommand() with
                | ARCValidateCommand.Validate validate -> validate
                | command -> failtestf "Expected validate, got %A" command

            [
                validate [|
                    "--validation-config-sha256"
                    (String.replicate 64 "a")
                |],
                false,
                "requires --validation-config"
                validate [| "--validation-config"; "config.yml" |],
                false,
                "requires --package"
                validate [|
                    "--package"
                    "configured"
                    "--validation-config"
                    "config.yml"
                |],
                false,
                "requires --package-version"
                validate [|
                    "--package"
                    "configured"
                    "--package-version"
                    "1.0.0"
                    "--validation-config"
                    "config.yml"
                |],
                true,
                "mutually exclusive"
            ]
            |> List.iter (fun (arguments, hasBoundary, expected) ->
                match ValidateArgs.validateCombination arguments hasBoundary with
                | Ok () -> failtestf "Expected combination failure containing '%s'." expected
                | Error message ->
                    Expect.stringContains message expected "Combination diagnostic"
            )
        }
    ]

namespace ARCValidate.API

open ARCValidate
open ARCValidate.CLIArguments
open ARCExpect
open ARCExpect.Configs
open ARCTokenization
open ARCValidationPackages

open Expecto
open System.IO
open Argu

open Spectre.Console

open ControlledVocabulary
open AnyBadge.NET

module ValidateAPI = 

    let validate (verbose: bool) (token: string option) (args: ParseResults<ValidateArgs>)=

        let root = 
            args.TryGetResult(ARC_Directory)
            |> Option.defaultValue (System.Environment.GetEnvironmentVariable("ARC_PATH")) // default to ARC_PATH if argument is not provided
            |> fun s -> if System.String.IsNullOrWhiteSpace(s) then System.Environment.CurrentDirectory else s // default to ./ if ARC_PATH is not set
            |> Path.GetFullPath
            |> fun p -> p.Replace("\\","/")
            |> fun p -> if not (p.EndsWith("/")) then p + "/" else p // ensure path ends with a slash

        let outPath = 
            args.TryGetResult(Out_Directory)
            |> Option.defaultValue root

        let package = 
            args.TryGetResult(Package)

        let version = 
            args.TryGetResult(Package_Version)

        match package with

        | None -> // Default call means validate schema conformity
            [
                ""
                "[yellow]running `arc-validate validate` without a validation package defaults to validating against the ARC specification.[/]"
                "If you want to validate against a specific validation packaget, you can:"
                "   - run [green]arc-validate package list[/] to get a list of installable packages."
                "   - run [green]arc-validate package install <your-desired-package-name>[/] to install a validation package."
                "   - run [green]arc-validate validate -p <your-desired-package-name>[/] to validate agaionst a validation package."
            ]
            |> List.iter AnsiConsole.MarkupLine
            
            let status = AnsiConsole.Status()
            let mutable exitCode = ExitCode.Success

            status.Start($"Performing validation against the ARC specification V2_Draft", fun ctx ->

                if verbose then

                    AnsiConsole.MarkupLine("LOG: Running in:")
                    AnsiConsole.Write(TextPath(Path.GetFullPath(root)))
                    AnsiConsole.MarkupLine("")
                    AnsiConsole.MarkupLine($"LOG: running validation against [bold underline green]ARC specification V2_Draft[/].")
                    AnsiConsole.MarkupLine($"LOG: Output path is:")
                    AnsiConsole.Write(TextPath(Path.GetFullPath(outPath)))
                    AnsiConsole.MarkupLine("")

                let cases = ARCSpecification.V2_Draft.validationCases root 
                
                let outDirBadge = System.IO.Path.Combine(root, "ARC_specification_V2_Draft.svg")
                let outDirResXml =System.IO.Path.Combine(root, "ARC_specification_V2_Draft.xml")

                let results = ARCExpect.Execute.Validation cases
                    
                results
                |> Execute.JUnitSummaryCreation(outDirResXml)

                results
                |> Execute.BadgeCreation(outDirBadge, "ARC specification V2_Draft", "passing", DefaultColor=AnyBadge.NET.Color.GREEN)

                if results.successful then
                    exitCode <- ExitCode.Success
                else
                    exitCode <- ExitCode.CriticalTestFailure
            )

        | Some packageName -> // Validate against a specific package

            let status = AnsiConsole.Status()
            let mutable exitCode = ExitCode.Success

            let isRelease = args.TryGetResult(ValidateArgs.Preview).IsSome |> not

            let packageMessagePrefix = if isRelease then "" else "preview "

            status.Start($"Performing validation against the {packageMessagePrefix}{packageName} package", fun ctx ->

                if verbose then
                    AnsiConsole.MarkupLine("LOG: Running in:")
                    AnsiConsole.Write(TextPath(Path.GetFullPath(root)))
                    AnsiConsole.MarkupLine("")
                
                match ARCValidationPackages.API.Common.GetSyncedConfigAndCache(?Token = token) with
                | Error e -> 
                    PackageAPI.printGetSyncedConfigAndCacheError e
                    exitCode <- ExitCode.InternalError

                | Ok (config, avprCache, previewCache) -> 
                    let package = 
                        match version with 
                        | Some semver -> 
                            if isRelease then 
                                PackageCache.tryGetPackage packageName semver avprCache
                            else
                                PackageCache.tryGetPackage packageName semver previewCache
                        | None -> 
                            if isRelease then 
                                PackageCache.tryGetLatestPackage packageName avprCache
                            else
                                PackageCache.tryGetLatestPackage packageName previewCache

                    match package with
                    | Some validationPackage ->
                        if verbose then
                            AnsiConsole.MarkupLine($"LOG: [green]validation package [bold underline]{packageName}[/] is installed locally:[/]")
                            AnsiConsole.MarkupLine($"LOG: {validationPackage.PrettyPrint()}")
                            AnsiConsole.MarkupLine($"LOG: running validation against [bold underline green]{packageName}[/].")
                            AnsiConsole.MarkupLine($"LOG: Output path is:")
                            AnsiConsole.Write(TextPath(Path.GetFullPath(outPath)))
                            AnsiConsole.MarkupLine("")

                        let result = ScriptExecution.runPackageScriptWithArgs validationPackage [| "-i"; root; "-o"; outPath |]

                        if result.OK then
                            exitCode <- ExitCode.Success
                        else
                            exitCode <- ExitCode.InternalError

                    | None -> 
                        AnsiConsole.MarkupLine($"[red]Package {packageName} not installed. You can run run [green]arc-validate package install <your-desired-package-name>[/] to install a validation package.[/]")
                        exitCode <- ExitCode.InternalError
            )
        try

            ExitCode.Success

        with e ->
            ExitCode.CriticalTestFailure
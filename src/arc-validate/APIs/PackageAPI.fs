namespace ARCValidate.API

open ARCValidate
open ARCValidate.CLIArguments
open ARCValidate.CLICommands
open ARCExpect
open ARCValidationPackages

open Argu

type PackageAPI = 

    static member printPackageInstallError (e: PackageInstallError) =
        match e with
        | PackageNotFound p -> printfn $"Package {p} not found. Your package index might be out of date. Consider updating the index via arc-validate package update-index."
        | DownloadError (p, msg) -> printfn $"Error downloading package {p}: {msg}."

    static member printGetSyncedConfigAndCacheError (e: GetSyncedConfigAndCacheError) =
         match e with
         | SyncError msg -> printfn $"Error syncing config and cache: {msg}"

    static member Install(
        args: ParseResults<PackageInstallArgs>,
        ?Verbose: bool,
        ?Token: string
    ) = 
        match ARCValidationPackages.API.GetSyncedConfigAndCache(?Token = Token) with
        | Error e -> 
            PackageAPI.printGetSyncedConfigAndCacheError e
            ExitCode.InternalError

        | Ok (config, cache) -> 

            let packageName = args.TryGetResult(PackageInstallArgs.Package).Value

            match (API.InstallPackage(config, cache, packageName, ?Verbose = Verbose, ?Token = Token)) with
            | Ok msg ->
                printfn $"{msg}"
                ExitCode.Success

            | Error e ->
                PackageAPI.printPackageInstallError e
                ExitCode.InternalError

    static member Uninstall(
        args: ParseResults<PackageUninstallArgs>,
        ?Verbose: bool,
        ?Token: string
    ) = 

        match ARCValidationPackages.API.GetSyncedConfigAndCache(?Token = Token) with
        | Error e -> 
            PackageAPI.printGetSyncedConfigAndCacheError e
            ExitCode.InternalError

        | Ok (config, cache) -> 
            let verbose = defaultArg Verbose false
            let packageName = args.TryGetResult(PackageUninstallArgs.Package).Value

            match (API.UninstallPackage(cache, packageName, verbose)) with
            | Ok msg ->
                printfn $"{msg}"
                ExitCode.Success
            | Error e ->
                match e with
                | PackageNotInstalled msg ->
                    printfn $"{msg}"
                    ExitCode.Success
                | IOError m ->
                    printfn $"Error uninstalling package {packageName}."
                    if verbose then printfn $"{m}"
                    ExitCode.InternalError

    static member List(
        args: ParseResults<PackageListArgs>,
        ?Verbose: bool, 
        ?Token: string
    ) = 

        match ARCValidationPackages.API.GetSyncedConfigAndCache(?Token = Token) with
        | Error e -> 
            PackageAPI.printGetSyncedConfigAndCacheError e
            ExitCode.InternalError

        | Ok (config, cache) -> 
            let verbose = defaultArg Verbose false

            let printCachedPackageList (packages: ARCValidationPackage list) =
                packages
                |> fun p -> 
                    if p.Length = 0 then 
                        printfn $"No validation packages installed."
                    else 
                        p |> List.iteri (fun i p -> printfn $"{System.Environment.NewLine}[{i}]:\tName: {p.Name}{System.Environment.NewLine}\tInstalled on: {p.CacheDate}{System.Environment.NewLine}\tat: {p.LocalPath}")

            let printIndexedPackageList (packages: ValidationPackageIndex list) =
                packages
                |> fun p -> 
                    if p.Length = 0 then 
                        printfn $"No validation packages indexed."
                    else 
                        p |> List.iteri (fun i p -> printfn $"{System.Environment.NewLine}[{i}]:\tName: {p.FileName}{System.Environment.NewLine}\tLast updated: {p.LastUpdated}")

            match (args.TryGetResult(Installed), args.TryGetResult(Indexed)) with
            | None, None | Some Installed, None | None, Some Installed->
                match API.ListCachedPackages(cache, verbose) with
                | Ok packages ->
                    printf $"Installed validation packages:"
                    printCachedPackageList packages
                    ExitCode.Success
                | Error e ->
                    printfn $"Error listing packages."
                    if verbose then printfn $"{e}"
                    ExitCode.InternalError
            | None, Some Indexed | Some Indexed, None->
                match API.ListIndexedPackages(config, verbose) with
                | Ok packages ->
                    printf $"Indexed validation packages:"
                    printIndexedPackageList packages
                    ExitCode.Success
                | Error e ->
                    printfn $"Error listing packages."
                    if verbose then printfn $"{e}"
                    ExitCode.InternalError
            | Some Indexed, Some Installed | Some Installed, Some Indexed ->
                let installed = API.ListCachedPackages(cache, verbose)
                let cached = API.ListIndexedPackages(config, verbose)

                match (installed, cached) with
                | Ok installed, Ok cached ->
                    printf $"Installed validation packages:"
                    printCachedPackageList installed
                    printf $"Indexed validation packages:"
                    printIndexedPackageList cached
                    ExitCode.Success
                | Error e, _ ->
                    printfn $"Error listing installed packages."
                    if verbose then printfn $"{e}"
                    ExitCode.InternalError
                | _, Error e ->
                    printfn $"Error listing indexed packages."
                    if verbose then printfn $"{e}"
                    ExitCode.InternalError

    static member UpdateIndex(
        ?Verbose: bool,
        ?Token: string
    ) = 
        match ARCValidationPackages.API.GetSyncedConfigAndCache(?Token = Token) with
        | Error e -> 
            PackageAPI.printGetSyncedConfigAndCacheError e
            ExitCode.InternalError

        | Ok (config, _) -> 
            let verbose = defaultArg Verbose false

            match API.UpdateIndex(config, ?Token = Token) with
            | Ok _ ->
                printfn $"Updated package index."
                ExitCode.Success
            | Error e ->
                printfn $"Error updating package index."
                if verbose then printfn $"{e}"
                ExitCode.InternalError
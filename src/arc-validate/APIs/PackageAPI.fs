namespace ARCValidate.API

open ARCValidate
open ARCValidate.CLIArguments
open ARCValidate.CLICommands
open ArcValidation
open ARCValidationPackages
open ARCValidationPackages.API
open ARCValidationPackages.Errors

open Argu

type PackageAPI = 

    static member internal getSyncedConfigAndCache() =
        let config = Config.get()
        config |> Config.write()
        let cache = PackageCache.get()
        cache |> PackageCache.write()

        config, cache

    static member printPackageInstallError (e: PackageInstallError) =
        match e with
        | PackageNotFound p -> printfn $"Package {p} not found. Your package index might be out of date. Consider updating the index via arc-validate package update-index."
        | DownloadError (p, msg) -> printfn $"Error downloading package {p}: {msg}."

    static member install (verbose: bool) (args: ParseResults<PackageInstallArgs>) = 
        
        let config, cache = PackageAPI.getSyncedConfigAndCache()

        let packageName = args.TryGetResult(PackageInstallArgs.Package).Value

        match (installPackage verbose config cache packageName) with
        | Ok msg ->
            printfn $"{msg}"
            ExitCode.Success

        | Error e ->
            PackageAPI.printPackageInstallError e
            ExitCode.InternalError

    static member uninstall (verbose: bool) (args: ParseResults<PackageUninstallArgs>) : ExitCode = 

        let config, cache = PackageAPI.getSyncedConfigAndCache()

        let packageName = args.TryGetResult(PackageUninstallArgs.Package).Value

        match (uninstallPackage verbose config cache packageName) with
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

    static member list (verbose: bool) (args: ParseResults<PackageListArgs>) : ExitCode = 

        let config, cache = PackageAPI.getSyncedConfigAndCache()

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
                    p |> List.iteri (fun i p -> printfn $"{System.Environment.NewLine}[{i}]:\tName: {p.Name}{System.Environment.NewLine}\tLast updated: {p.LastUpdated}")

        match (args.TryGetResult(Installed), args.TryGetResult(Indexed)) with
        | None, None | Some Installed, None | None, Some Installed->
            match listCachedPackages verbose cache with
            | Ok packages ->
                printf $"Installed validation packages:"
                printCachedPackageList packages
                ExitCode.Success
            | Error e ->
                printfn $"Error listing packages."
                if verbose then printfn $"{e}"
                ExitCode.InternalError
        | None, Some Indexed | Some Indexed, None->
            match listIndexedPackages verbose config with
            | Ok packages ->
                printf $"Indexed validation packages:"
                printIndexedPackageList packages
                ExitCode.Success
            | Error e ->
                printfn $"Error listing packages."
                if verbose then printfn $"{e}"
                ExitCode.InternalError
        | Some Indexed, Some Installed | Some Installed, Some Indexed ->
            let installed = listCachedPackages verbose cache
            let cached = listIndexedPackages verbose config

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

    static member updateIndex (verbose: bool) : ExitCode = 
        let config, _ = PackageAPI.getSyncedConfigAndCache()
        match updateIndex config with
        | Ok _ ->
            printfn $"Updated package index."
            ExitCode.Success
        | Error e ->
            printfn $"Error updating package index."
            if verbose then printfn $"{e}"
            ExitCode.InternalError
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

    static member uninstall (verbose: bool) (args: ParseResults<PackageUninstallArgs>) : ExitCode = raise (System.NotImplementedException())

    static member list (verbose: bool) (args: ParseResults<PackageListArgs>) : ExitCode = raise (System.NotImplementedException())

    static member updateIndex (verbose: bool) : ExitCode = raise (System.NotImplementedException())


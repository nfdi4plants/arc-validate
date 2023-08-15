#r "nuget: FsHttp, 11.0.0"
#r "nuget: Fake.DotNet.Cli, 6.0.0"
#r "../src/ARCValidationPackages/bin/Release/net6.0/ARCValidationPackages.dll"

open ARCValidationPackages
open System.IO
open Fake.DotNet

let config = Config.initDefault()

config |> Config.write()

let cache = PackageCache()

config.PackageIndex[0]
|> fun pI ->
    let script = GitHubAPI.downloadPackageScript(pI)

    let package = pI |> ARCValidationPackage.ofPackageIndex

    cache 
    |> PackageCache.addPackage(package)
    |> PackageCache.write()

    File.WriteAllText(package.LocalPath, script)

open Fake.DotNet

cache["test"]
|> fun p -> 
    DotNet.exec 
        (fun p -> 
            {
                p with
                    RedirectOutput = true
                    PrintRedirectedOutput = true
            }
        )
        "fsi" 
        p.LocalPath
    
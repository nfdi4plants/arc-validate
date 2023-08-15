#r "nuget: FsHttp, 11.0.0"
#r "nuget: Fake.DotNet.Fsi, 6.0.0"
#r "../src/ARCValidationPackages/bin/Release/net6.0/ARCValidationPackages.dll"

open ARCValidationPackages
open System.IO

let config = Config.initDefault()

config |> Config.write

let cache = PackageCache()

config.PackageIndex[2]
|> fun i ->
    let script = GitHubAPI.downloadPackageScript(i.Name)

    let scriptPath = Path.Combine(config.PackageCacheFolder, $"""{i.Name.Split("/")[1]}""")

    let name = Path.GetFileNameWithoutExtension(scriptPath)

    cache.Add(name, ARCValidationPackage.create(i.Name, i.LastUpdated, scriptPath))

    File.WriteAllText(scriptPath, script)

cache
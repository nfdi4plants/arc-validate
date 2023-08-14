#r "nuget: FsHttp, 11.0.0"
#r "nuget: Fake.DotNet.Fsi, 6.0.0"
#r "../src/ARCValidationPackages/bin/Release/net6.0/ARCValidationPackages.dll"

open ARCValidationPackages
open System.IO

let c = Config.initDefault()

c.PackageIndex[2]
|> fun i ->
    let script = GitHubAPI.downloadPackageScript(i.Name)

    let scriptPath = Path.Combine(c.PackageCacheFolder, $"""{i.Name.Split("/")[0]}.fsx""")

    File.WriteAllText(scriptPath, script)

Defaults.PACKAGE_CACHE_FOLDER()
Directory.Exists(Defaults.PACKAGE_CACHE_FOLDER())
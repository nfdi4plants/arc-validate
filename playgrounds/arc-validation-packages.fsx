#r "nuget: FsHttp, 11.0.0"
#r "nuget: Fake.DotNet.Fsi, 6.0.0"
#r "../src/ARCValidationPackages/bin/Release/net6.0/ARCValidationPackages.dll"

open ARCValidationPackages

let c = Config.initDefault()

c
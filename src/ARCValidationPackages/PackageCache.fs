namespace ARCValidationPackages

open System.Collections.Generic

type PackageCache =
    inherit Dictionary<string, ARCValidationPackage>

    new () = { inherit Dictionary<string, ARCValidationPackage>() }

    new (packages: IEnumerable<KeyValuePair<string,ARCValidationPackage>>) = { inherit Dictionary<string, ARCValidationPackage>(packages) }

    new(packages: seq<string * ARCValidationPackage>) = 
        let kv = packages |> Seq.map (fun (k,v) -> KeyValuePair.Create(k,v))
        PackageCache(kv)

    static member create (packages: seq<string * ARCValidationPackage>) =
        new PackageCache(packages)

    static member addPackage (package: ARCValidationPackage) (cache: PackageCache) =
        cache.Add(package.Name, package)
        cache

    static member removePackage (name: string) (cache: PackageCache) =
        cache.Remove(name) |> ignore
        cache

    static member write (path: string) (cache: PackageCache) =
        // to-do:
        // - convert to json and write to file
        raise (System.NotImplementedException())

    static member read (path: string) =
        // to-do:
        // - load cache from json file in cache location
        // - init object with loaded values
        raise (System.NotImplementedException())
namespace ARCValidate.PackageManagement

open System
open System.IO

module internal AtomicFile =

    let private writeWith (writeTemporaryFile: string -> unit) (path: string) =
        let file = FileInfo(path)
        file.Directory.Create()

        let temporaryPath =
            Path.Combine(
                file.DirectoryName,
                $".{file.Name}.{Guid.NewGuid():N}.tmp"
            )

        try
            writeTemporaryFile temporaryPath
            File.Move(temporaryPath, path, true)
        finally
            if File.Exists(temporaryPath) then
                File.Delete(temporaryPath)

    let writeAllText (path: string) (content: string) =
        writeWith (fun temporaryPath -> File.WriteAllText(temporaryPath, content)) path

    let writeAllBytes (path: string) (content: byte array) =
        writeWith (fun temporaryPath -> File.WriteAllBytes(temporaryPath, content)) path
        

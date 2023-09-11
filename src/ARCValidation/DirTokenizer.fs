namespace ArcValidation

open StaticCvTerms


/// TODO: MOve to the right place!!!

module DirTokenizer = 

    open System.IO
    open System
    open ControlledVocabulary

    let tokenizeRelDirectory (rootPath:string) =
        let root = System.Uri(rootPath)
        seq {
                for dir in Directory.EnumerateDirectories(rootPath, "*", SearchOption.AllDirectories) do        
                    let currentUri =  System.Uri(dir)
                    yield CvParam(IoTerms.DirectoryPath,root.MakeRelativeUri(currentUri).ToString())
            }

    let tokenizeDirectory  (rootPath:string) =
        seq {
                for dir in Directory.EnumerateDirectories(rootPath, "*", SearchOption.AllDirectories) do        
                    yield CvParam(IoTerms.DirectoryPath,dir)
            }    


    let tokenizeFilesWithRelDirectory (rootPath:string) =
        let root = System.Uri(rootPath)
        seq {
                for file in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories) do        
                    let currentFileUri =  System.Uri(file)
                    yield CvParam(IoTerms.FilePath,root.MakeRelativeUri(currentFileUri).ToString())
            }

    let tokenizeFiles (rootPath:string) =
        seq {
                for file in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories) do        
                    yield CvParam(IoTerms.FilePath,file)
            }
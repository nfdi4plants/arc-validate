// testin' around

#r "nuget: BioFSharp.IO"
#r "nuget: FSharpAux.IO"


let s = FSharpAux.IO.SeqIO.Seq.fromFile @"C:\Repos\nfdi4plants\arc-validate\ErrorClassOntology.obo"

let terms = BioFSharp.IO.Obo.parseOboTerms s |> Seq.toArray


// 'mplementin' around

type MissingFilesystemEntry =
    static member error = printfn "error"

type MissingEntity =
    static member MissingFilesystemEntry = MissingFilesystemEntry

type Error =
    static member MissingEntity = MissingEntity
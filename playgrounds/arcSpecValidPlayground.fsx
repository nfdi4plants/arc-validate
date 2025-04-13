#r "nuget: ARCTokenization"
#r "nuget: ARCExpect"
#r "nuget: CWLDotNet"


open ARCTokenization
open ControlledVocabulary
open System.IO
open System.Text.RegularExpressions
open ARCExpect
open FSharpAux
open CWLDotNet


let arcDir = @"C:\Users\revil\OneDrive\CSB-Stuff\NFDI\testARC37"

let fs = ARCTokenization.FileSystem.parseARCFileSystem arcDir |> List.ofSeq

fs
|> List.iteri (
    fun i cvp -> cvp.Value.ToString() |> printfn "%i: %s" i
)

let pattern = System.Text.RegularExpressions.Regex(@"^assays/([^/]+)/dataset/([^/]+)$")

pattern.Match (fs[67].Value |> ControlledVocabulary.ParamValue.getValueAsString)

let cwls =
    fs
    |> Seq.filter (
        fun x -> 
            x.Name = StructuralOntology.AFSO.``CWL File``.Name && 
            x.Value
            |> ParamValue.getValue
            |> string
            |> fun (x : string) -> x.Split [|'/'|]
            |> fun x -> 
                match x with 
                | [|"workflows";_;_|] -> true
                | _ -> false
    )
    |> List.ofSeq

let cwlsContents =
    cwls
    |> Seq.map (
        fun cwlPath ->
            Path.Combine(arcDir, Param.getValueAsString cwlPath)
            |> File.ReadAllText
    )
    |> List.ofSeq

let isCwlVersionAtLeast1_2 str =
    let pattern = Regex(@"^cwlVersion:\s*v1\.2(?:\.0-dev[1-5])?$", RegexOptions.Multiline)
    let rectifiedStr = String.replace "\r\n" "\n" str    // Windows special character '\013' (Carriage Return) must be purged
    (pattern.Match rectifiedStr).Success

let runDir = 
    fs 
    |> Seq.filter (Param.getCvName >> (=) StructuralOntology.AFSO.``Workflow Directory``.Name)
    // |> Seq.filter (fun x -> x.Name = StructuralOntology.AFSO.``Workflow Directory``.Name)
    |> Seq.toList

let getTextFileContent arcDir relPathParam =
    Path.Combine(arcDir, relPathParam)
    |> File.ReadAllText

cwls
|> Validate.ParamCollection.SatisfiesPredicate (
    fun x ->
        x
        |> Seq.forall (
            Param.getValueAsString
            >> getTextFileContent arcDir
            >> isCwlVersionAtLeast1_2
        )
)

let r =
    cwls
    |> List.map (
        Param.getValueAsString
        >> getTextFileContent arcDir
        >> isCwlVersionAtLeast1_2
    )

let r2 = Param.getValueAsString cwls.Head |> getTextFileContent arcDir
r2 |> isCwlVersionAtLeast1_2

isCwlVersionAtLeast1_2 "cwlVersion: v1.2"
isCwlVersionAtLeast1_2 "cwlVersion: v1.2

# What type"
isCwlVersionAtLeast1_2 """cwlVersion: v1.2

# What type of CWL process we have in this document.
class: CommandLineTool
# This CommandLineTool executes the linux "echo" command-line tool.
baseCommand: echo

# The inputs for this process.
inputs:
  message:
    type: string
    # A default value that can be overridden, e.g. --message "Hola mundo"
    default: "Hello World"
    # Bind this message value as an argument to "echo".
    inputBinding:
      position: 1
outputs: []"""

r2.ToCharArray()

"""cwlVersion: v1.2

# What type of CWL process we have in this document.
class: CommandLineTool
# This CommandLineTool executes the linux "echo" command-line tool.
baseCommand: echo

# The inputs for this process.
inputs:
  message:
    type: string
    # A default value that can be overridden, e.g. --message "Hola mundo"
    default: "Hello World"
    # Bind this message value as an argument to "echo".
    inputBinding:
      position: 1
outputs: []""".ToCharArray()

let cwlDoc = CWLDotNet.RootLoader.LoadDocument(r2, Path.Combine(arcDir, Param.getValueAsString cwls.Head))
cwlDoc.Value

let cwlDoc2 = RootLoader.LoadDocument(File.ReadAllText @"C:\Users\revil\OneDrive\CSB-Stuff\NFDI\testARC37\runs\run1_singleOutput\run.cwl", @"C:\Users\revil\OneDrive\CSB-Stuff\NFDI\testARC37\runs\run1_singleOutput\run.cwl")

cwlDoc2.Value
CWLDotNet.CommandOutputRecordField.FromDoc(cwlDoc2.Value, @"C:\Users\revil\OneDrive\CSB-Stuff\NFDI\testARC37\runs\run1_singleOutput\run.cwl", LoadingOptions())

let std = Study.parseProcessGraphColumnsFromTokens arcDir 
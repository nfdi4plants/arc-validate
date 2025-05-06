#r "nuget: ARCtrl"


open ARCtrl


let arcEmpty = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/empty"
// ^ loads well even if Investigation identifier is empty

arcEmpty.FileSystem.Tree
arcEmpty.ISA.Value


let arcInvestigationMissing = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/investigationMissing"
// ^ raises error: 
//System.Exception: Could not load ARC, failed with the following errors Error reading contract isa.investigation.xlsx: Could not find file '../tests/arc-validate.Tests/fixtures/arcs/errorARCs/investigationMissing\isa.investigation.xlsx'.
//   at Microsoft.FSharp.Core.PrintfModule.PrintFormatToStringThenFail@1448.Invoke(String message)
//   at <StartupCode$ARCtrl>.$ARC.loadAsync@272-1.Invoke(FSharpResult`2 result) in C:\Users\HLWei\source\repos\ARC_tools\ARCtrl\src\ARCtrl\ARC.fs:line 276
//   at Microsoft.FSharp.Control.AsyncPrimitives.CallThenInvokeNoHijackCheck[a,b](AsyncActivation`1 ctxt, b result1, FSharpFunc`2 userCode) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 528
//   at ARCtrl.Contract.fulfillReadContractAsync@29-14.Invoke(AsyncActivation`1 ctxt)
//   at Microsoft.FSharp.Control.Trampoline.Execute(FSharpFunc`2 firstAction) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 112
//--- End of stack trace from previous location ---
//   at Microsoft.FSharp.Control.AsyncResult`1.Commit() in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 454
//   at Microsoft.FSharp.Control.AsyncPrimitives.QueueAsyncAndWaitForResultSynchronously[a](CancellationToken token, FSharpAsync`1 computation, FSharpOption`1 timeout) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 1140
//   at Microsoft.FSharp.Control.FSharpAsync.RunSynchronously[T](FSharpAsync`1 computation, FSharpOption`1 timeout, FSharpOption`1 cancellationToken) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 1511
//   at <StartupCode$FSI_0011>.$FSI_0011.main@() in C:\Repos\nfdi4plants\arc-validate\arcSpecValPlayground.fsx:line 12
//   at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
//   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)
//Stopped due to error


let arcInvestigationIdentifierMissing = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/investigationIdentifierMissing"
// ^ loads well even if Investigation identifier is missing (= row does not exist) and parses this as empty string

arcInvestigationIdentifierMissing.ISA.Value


let arcInvestigationAllFieldsMissing = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/investigationAllFieldsMissing"
// ^ raises error:
//System.Exception: Could not read investigation from spreadsheet: emptyInvestigationFile
//   at Microsoft.FSharp.Core.PrintfModule.PrintFormatToStringThenFail@1448.Invoke(String message)
//   at ARCtrl.Spreadsheet.ArcInvestigationExtensions.ArcInvestigation.fromFsWorkbook.Static(FsWorkbook doc) in C:\Users\HLWei\source\repos\ARC_tools\ARCtrl\src\Spreadsheet\ArcInvestigation.fs:line 239
//   at ARCtrl.Contract.InvestigationContractExtensions.ArcInvestigation.tryFromReadContract.Static(Contract c) in C:\Users\HLWei\source\repos\ARC_tools\ARCtrl\src\Contract\ArcInvestigation.fs:line 54
//   at Microsoft.FSharp.Collections.ArrayModule.Choose[T,TResult](FSharpFunc`2 chooser, T[] array) in D:\a\_work\1\s\src\FSharp.Core\array.fs:line 632
//   at ARCtrl.ARCAux.getArcInvestigationFromContracts(Contract[] contracts) in C:\Users\HLWei\source\repos\ARC_tools\ARCtrl\src\ARCtrl\ARC.fs:line 35
//   at ARCtrl.ARC.SetISAFromContracts(Contract[] contracts) in C:\Users\HLWei\source\repos\ARC_tools\ARCtrl\src\ARCtrl\ARC.fs:line 434
//   at <StartupCode$ARCtrl>.$ARC.tryLoadAsync@114-3.Invoke(FSharpResult`2 fulFilledContracts) in C:\Users\HLWei\source\repos\ARC_tools\ARCtrl\src\ARCtrl\ARC.fs:line 116
//   at Microsoft.FSharp.Control.AsyncPrimitives.CallThenInvokeNoHijackCheck[a,b](AsyncActivation`1 ctxt, b result1, FSharpFunc`2 userCode) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 528
//   at ARCtrl.Contract.fulfillReadContractAsync@20-6.Invoke(AsyncActivation`1 ctxt)
//   at Microsoft.FSharp.Control.Trampoline.Execute(FSharpFunc`2 firstAction) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 112
//--- End of stack trace from previous location ---
//   at Microsoft.FSharp.Control.AsyncResult`1.Commit() in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 454
//   at Microsoft.FSharp.Control.AsyncPrimitives.QueueAsyncAndWaitForResultSynchronously[a](CancellationToken token, FSharpAsync`1 computation, FSharpOption`1 timeout) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 1140
//   at Microsoft.FSharp.Control.FSharpAsync.RunSynchronously[T](FSharpAsync`1 computation, FSharpOption`1 timeout, FSharpOption`1 cancellationToken) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 1511

//   at <StartupCode$FSI_0015>.$FSI_0015.main@() in C:\Repos\nfdi4plants\arc-validate\arcSpecValPlayground.fsx:line 38
//   at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
//   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)
//Stopped due to error


let arcInvestigationAllKeysMissing = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/investigationAllKeysMissing"
// ^ loads well even if all keys in the Investigation file are missing (everything parsed as empty string or seq)

arcInvestigationAllKeysMissing.ISA


let arcInvestigationInvestigationSectionMissing = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/investigationInvestigationSectionMissing"
// ^ loads well even if all fields in the Investigation section of the Investigation file are missing (all parsed as empty)

arcInvestigationInvestigationSectionMissing.ISA.Value


let arcInvestigationAllFieldsMissingButOne = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/investigationAllFieldsMissingButOne"
// ^ loads well even if all fields in the Investigation file are missing but one (all parsed as empty)

arcInvestigationAllFieldsMissingButOne.FileSystem
arcInvestigationAllFieldsMissingButOne.ISA
arcInvestigationAllFieldsMissingButOne.ISA.Value.Assays |> Seq.length


let arcInvestigation1FieldShifted = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/investigation1FieldShifted"
// ^ loads well even if 1 field (Investigation title) is shifted to the right but the field itself is not recognized and parsed as empty

arcInvestigation1FieldShifted.ISA


let arcInvestigationIdentifierShifted = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/investigationIdentifierShifted"
// ^ loads well even if identifier field is shifted to the right but the field itself is not recognized and parsed as empty

arcInvestigationIdentifierShifted.ISA


let arcInvestigation1ValueShifted = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/investigation1ValueShifted"
// ^ loads well even if 1 value (Investigation title) is shifted to the right but the value itself is not recognized and parsed as empty

arcInvestigation1ValueShifted.ISA


let arcInvestigationIdentifierValueShifted = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/investigationIdentifierValueShifted"
// ^ loads well even if identifier value is shifted to the right but the value itself is not recognized and parsed as empty

arcInvestigationIdentifierValueShifted.ISA


let arcStudyAllFieldsMissing = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/studyAllFieldsMissing"
// ^ loads well even if all fields in the Study metadata sheet are missing (parsed as empty seq inside the seq of Studies)

arcStudyAllFieldsMissing.ISA


let arcStudy1FieldShifted = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/study1FieldShifted"
// ^ loads well even if 1 field is shifted to the right but the field itself is not recognized and parsed as empty

arcStudy1FieldShifted.ISA


let arcStudyIdentifierShifted = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/studyIdentifierShifted"
// ^ loads well even with Study identifier field shifted but StudyIdentifiers now has the string "MISSING_IDENTIFIER_18962cc4-f294-470e-90a4-12cd50714927"

arcStudyIdentifierShifted.ISA


let arcStudyIdentifierValueShifted = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/studyIdentifierValueShifted"
// ^ loads well even with Study identifier field shifted but StudyIdentifiers now has the string "MISSING_IDENTIFIER_c5790905-86f5-4225-8f1e-23e0b7441921"

arcStudyIdentifierValueShifted.ISA


let arcStudyAllKeysMissing = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/studyAllKeysMissing"
// ^ loads well even if all keys in the Study metadata sheet are missing (parsed as empty seq inside the seq of Studies)

arcStudyAllKeysMissing.ISA


let arcAssayAllFieldsMissing = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/assayAllFieldsMissing"
// ^ raises error:
//System.Exception: Could not parse assay: 
//Failed while parsing metadatasheet: empty assay metadata sheet
//   at Microsoft.FSharp.Core.PrintfModule.PrintFormatToStringThenFail@1448.Invoke(String message)
//   at ARCtrl.Spreadsheet.ArcAssayExtensions.ArcAssay.fromFsWorkbook.Static(FsWorkbook doc) in C:\Users\HLWei\source\repos\ARC_tools\ARCtrl\src\Spreadsheet\ArcAssay.fs:line 134
//   at ARCtrl.Contract.AssayContractExtensions.ArcAssay.tryFromReadContract.Static(Contract c) in C:\Users\HLWei\source\repos\ARC_tools\ARCtrl\src\Contract\ArcAssay.fs:line 58
//   at Microsoft.FSharp.Collections.ArrayModule.Choose[T,TResult](FSharpFunc`2 chooser, T[] array) in D:\a\_work\1\s\src\FSharp.Core\array.fs:line 632
//   at ARCtrl.ARC.SetISAFromContracts(Contract[] contracts) in C:\Users\HLWei\source\repos\ARC_tools\ARCtrl\src\ARCtrl\ARC.fs:line 438
//   at <StartupCode$ARCtrl>.$ARC.tryLoadAsync@114-3.Invoke(FSharpResult`2 fulFilledContracts) in C:\Users\HLWei\source\repos\ARC_tools\ARCtrl\src\ARCtrl\ARC.fs:line 116
//   at Microsoft.FSharp.Control.AsyncPrimitives.CallThenInvokeNoHijackCheck[a,b](AsyncActivation`1 ctxt, b result1, FSharpFunc`2 userCode) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 528
//   at ARCtrl.Contract.fulfillReadContractAsync@20-6.Invoke(AsyncActivation`1 ctxt)
//   at Microsoft.FSharp.Control.Trampoline.Execute(FSharpFunc`2 firstAction) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 112
//--- End of stack trace from previous location ---
//   at Microsoft.FSharp.Control.AsyncResult`1.Commit() in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 454
//   at Microsoft.FSharp.Control.AsyncPrimitives.QueueAsyncAndWaitForResultSynchronously[a](CancellationToken token, FSharpAsync`1 computation, FSharpOption`1 timeout) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 1140
//   at Microsoft.FSharp.Control.FSharpAsync.RunSynchronously[T](FSharpAsync`1 computation, FSharpOption`1 timeout, FSharpOption`1 cancellationToken) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 1511
//   at <StartupCode$FSI_0017>.$FSI_0017.main@() in C:\Repos\nfdi4plants\arc-validate\arcSpecValPlayground.fsx:line 82
//   at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
//   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)
//Stopped due to error


let arcAssayAllKeysMissing = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/assayAllKeysMissing"

arcAssayAllKeysMissing.ISA
// ^ loads well even if all keys in the Assay metadata sheet are missing (parsed as empty seq inside the seq of Assays)


let arcAssay1FieldShifted = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/assay1FieldShifted"
// ^ loads well but Assay is not parsed :|

arcAssay1FieldShifted.ISA.Value.Assays.Item 0


let arcAssay1ValueShifted = ARC.load "../tests/arc-validate.Tests/fixtures/arcs/errorARCs/assay1ValueShifted"
// ^ loads well but Assay is not parsed :|

arcAssay1ValueShifted.ISA.Value.Assays





// ------------


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
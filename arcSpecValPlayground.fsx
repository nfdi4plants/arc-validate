#r "nuget: ARCtrl"


open ARCtrl


let arcEmpty = ARC.load @"C:\Users\olive\OneDrive\CSB-Stuff\NFDI\errorARCs\empty"
// ^ loads well even if Investigation identifier is empty

arcEmpty.FileSystem.Tree
arcEmpty.ISA.Value


let arcInvestigationMissing = ARC.load @"C:\Users\olive\OneDrive\CSB-Stuff\NFDI\errorARCs\investigationMissing"
// ^ raises error: 
//System.Exception: Could not load ARC, failed with the following errors Error reading contract isa.investigation.xlsx: Could not find file 'C:\Users\olive\OneDrive\CSB-Stuff\NFDI\errorARCs\investigationMissing\isa.investigation.xlsx'.
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


let arcInvestigationIdentifierMissing = ARC.load @"C:\Users\olive\OneDrive\CSB-Stuff\NFDI\errorARCs\investigationIdentifierMissing"
// ^ loads well even if Investigation identifier is missing (= row does not exist) and parses this as empty string

arcInvestigationIdentifierMissing.ISA.Value


let arcInvestigationAllFieldsMissing = ARC.load @"C:\Users\olive\OneDrive\CSB-Stuff\NFDI\errorARCs\investigationAllFieldsMissing"
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


let arcInvestigationInvestigationSectionMissing = ARC.load @"C:\Users\olive\OneDrive\CSB-Stuff\NFDI\errorARCs\investigationInvestigationSectionMissing"
// ^ loads well even if all fields in the Investigation section of the Investigation file are missing (all parsed as empty)

arcInvestigationInvestigationSectionMissing.ISA.Value


let arcInvestigationAllFieldsMissingButOne = ARC.load @""
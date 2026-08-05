let [<Literal>]PACKAGE_METADATA = """(*
---
Name: packed-fsi
Summary: summary
Description: description
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: false
Inputs:
  - id: echo
    type: string?
    inputBinding:
      prefix: --echo
      position: 0
      separate: true
---
*)"""

#i "nuget: __ARCEXPECT_SOURCE__"
#r "nuget: ARCExpect, __ARCEXPECT_VERSION__"

open ARCExpect

let metadata = Setup.Metadata(PACKAGE_METADATA)

let arguments = PackageArguments.fromCommandLine(metadata)

if
    arguments.ArcDirectory <> "packed-arc"
    || arguments.OutputDirectory <> "packed-out"
    || metadata.ProgrammingLanguage <> "FSharp"
    || arguments.TryGetString("echo") <> Some "literal; $(not-executed)"
then
    failwith "F# Interactive package arguments were not normalized correctly."

module ValidateArc

open OntologyHelperFunctions
open CheckFilesystemStructure
open Expecto

let filesystem =
    testList "Filesystem" [
        testCase ".arc" <| fun () -> isPresent hasArcFolder (createMessage arcFolderPath None None)
        testList ".git" [
            testCase ".git folder"      <| fun () -> isPresent hasGitFolder     (createMessage gitFolderPath    None None)
            testCase "hooks folder"     <| fun () -> isPresent hasHooksFolder   (createMessage hooksPath        None None)
            testCase "objects folder"   <| fun () -> isPresent hasObjectsFolder (createMessage objectsPath      None None)
            testCase "refs folder"      <| fun () -> isPresent hasRefsFolder    (createMessage refsPath         None None)
        ]
    ]

let isaTests =
    testList "ISA" [
        testCase "Schema" <| fun () ->
            
    ]
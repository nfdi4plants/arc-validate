open Helpers

initializeContext()

open BasicTasks
open TestTasks
open PackageTasks
open ARCExpectPortableTasks
open DocsSampleTasks
open DocumentationTasks

// Force module initialization so every non-default target is registered.
let _runTests = runTests
let _runAutomatedTests = runAutomatedTests
let _testPortableARCExpect = testPortableARCExpect
let _runDocsSamples = runDocsSamples
let _buildDocs = buildDocs
let _watchDocs = watchDocs
let _watchApiDocs = watchApiDocs

[<EntryPoint>]
let main args = 
    runOrDefault buildSolution args


open Helpers

initializeContext()

open BasicTasks
open TestTasks
open PackageTasks
open ARCExpectPortableTasks
open DocsSampleTasks
open DocumentationTasks

// Force module initialization so every portable target is registered.
let _testPortableARCExpect = testPortableARCExpect
let _runDocsSamples = runDocsSamples
let _buildDocs = buildDocs
let _watchDocs = watchDocs
let _watchApiDocs = watchApiDocs

[<EntryPoint>]
let main args = 
    runOrDefault buildSolution args


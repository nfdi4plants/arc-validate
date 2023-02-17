module Tests

open Expecto
open System
open System.IO

[<Tests>]
let tests =
  testList "main" [
    testCase "test file exists in folder" ( fun _ ->
      let filePath = Path.Combine(Environment.CurrentDirectory,"test.txt")
      Expect.isTrue (File.Exists(filePath)) $"File {filePath} was not fount"
    )
    testCase "can read file in folder" ( fun _ ->
      let filePath = Path.Combine(Environment.CurrentDirectory,"test.txt")
      let fileContent = File.ReadAllText(filePath)
      Expect.equal fileContent "yes" "File content was not equal"
    )
  ]

module ReferenceObjects

open System.IO

module IO =
    let ``test arc 1 validation results`` = File.ReadAllText("fixtures/xml/io/test_arc_1/arc-validate-results.xml").ReplaceLineEndings()

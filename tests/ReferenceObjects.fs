module ReferenceObjects

open System.IO

module IO =
    let ``invenio test arc validation results`` = File.ReadAllText("fixtures/xml/inveniotestarc/arc-validate-results.xml").ReplaceLineEndings()

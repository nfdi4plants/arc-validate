namespace ARCExpect.JUnit

open ARCExpect
open Fable.Core

type private XmlAttribute =
    {
        Name: string
        Value: string
    }

type private XmlNode =
    | Element of name: string * attributes: XmlAttribute list * children: XmlNode list
    | Text of value: string

module private Xml =

    let attribute name value =
        { Name = name; Value = value }

    let element name attributes children =
        Element(name, attributes, children)

    let text value =
        Text value

    let private escape(value: string) =
        if isNull value then
            ""
        else
            value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;")

    let rec encode node =
        match node with
        | Text value -> escape value
        | Element(name, attributes, children) ->
            let encodedAttributes =
                attributes
                |> List.map (fun attribute ->
                    $" {attribute.Name}=\"{escape attribute.Value}\""
                )
                |> String.concat ""

            match children with
            | [] -> $"<{name}{encodedAttributes} />"
            | _ ->
                let encodedChildren = children |> List.map encode |> String.concat ""
                $"<{name}{encodedAttributes}>{encodedChildren}</{name}>"

    let document node =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + encode node

module private Formatting =

    let firstLine(value: string) =
        if isNull value then
            ""
        else
            value
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n')[0]

    let seconds(milliseconds: float) =
        let roundedMilliseconds = int (System.Math.Round(milliseconds))
        let wholeSeconds = roundedMilliseconds / 1000
        let remainder = abs (roundedMilliseconds % 1000)
        let fraction =
            if remainder < 10 then $"00{remainder}"
            elif remainder < 100 then $"0{remainder}"
            else string remainder

        $"{wholeSeconds}.{fraction}"

[<AttachMembers>]
type Writer private () =

    static member toXml(
        summary: RunSummary,
        ?Verbose: bool,
        ?SuiteName: string,
        ?SourceBranch: string,
        ?SourceCommitHash: string
    ) =
        let verbose = defaultArg Verbose false
        let suiteName = defaultArg SuiteName summary.SuiteName

        let caseNode (result: CaseResult) =
            let attributes = [
                Xml.attribute "name" result.FullName
                Xml.attribute "time" (Formatting.seconds result.DurationMilliseconds)
            ]

            let message =
                if verbose then result.Outcome.Message
                else Formatting.firstLine result.Outcome.Message

            let children =
                match result.Outcome.Kind with
                | CaseOutcomeKind.Passed -> []
                | CaseOutcomeKind.Failed ->
                    let details =
                        if
                            verbose
                            && not (System.String.IsNullOrWhiteSpace result.Outcome.StackTrace)
                        then
                            [ Xml.text result.Outcome.StackTrace ]
                        else
                            []

                    [ Xml.element "failure" [ Xml.attribute "message" message ] details ]
                | CaseOutcomeKind.Errored ->
                    let details =
                        if
                            verbose
                            && not (System.String.IsNullOrWhiteSpace result.Outcome.StackTrace)
                        then
                            [ Xml.text result.Outcome.StackTrace ]
                        else
                            []

                    [ Xml.element "error" [ Xml.attribute "message" message ] details ]
                | CaseOutcomeKind.Skipped ->
                    [ Xml.element "skipped" [ Xml.attribute "message" message ] [] ]
                | _ -> []

            Xml.element "testcase" attributes children

        let cases =
            summary.Cases
            |> Array.map caseNode
            |> Array.toList

        let sourceProperties = [
            match SourceBranch with
            | Some sourceBranch ->
                Xml.element "property" [
                    Xml.attribute "name" "SourceBranch"
                    Xml.attribute "value" sourceBranch
                ] []
            | None -> ()

            match SourceCommitHash with
            | Some sourceCommitHash ->
                Xml.element "property" [
                    Xml.attribute "name" "SourceCommitHash"
                    Xml.attribute "value" sourceCommitHash
                ] []
            | None -> ()
        ]

        let children =
            match sourceProperties with
            | [] -> cases
            | properties -> Xml.element "properties" [] properties :: cases

        let suiteAttributes = [
            Xml.attribute "name" suiteName
            Xml.attribute "tests" (string summary.Total)
            Xml.attribute "failures" (string summary.Failed)
            Xml.attribute "errors" (string summary.Errored)
            Xml.attribute "skipped" (string summary.Skipped)
            Xml.attribute "time" (Formatting.seconds summary.DurationMilliseconds)
        ]

        Xml.element "testsuite" suiteAttributes children
        |> fun suite -> Xml.element "testsuites" [] [ suite ]
        |> Xml.document

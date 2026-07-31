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

module private Xml =

    let attribute name value =
        { Name = name; Value = value }

    let element name attributes children =
        Element(name, attributes, children)

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
        ?SuiteName: string
    ) =
        let verbose = defaultArg Verbose false
        let suiteName = defaultArg SuiteName summary.SuiteName

        let outcomeOrder kind =
            match kind with
            | CaseOutcomeKind.Errored -> 3
            | CaseOutcomeKind.Failed -> 2
            | CaseOutcomeKind.Skipped -> 1
            | _ -> 0

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
                    [ Xml.element "failure" [ Xml.attribute "message" message ] [] ]
                | CaseOutcomeKind.Errored ->
                    [ Xml.element "error" [ Xml.attribute "message" message ] [] ]
                | CaseOutcomeKind.Skipped ->
                    [ Xml.element "skipped" [ Xml.attribute "message" message ] [] ]
                | _ -> []

            Xml.element "testcase" attributes children

        let cases =
            summary.Cases
            |> Array.sortByDescending (fun result ->
                outcomeOrder result.Outcome.Kind,
                result.DurationMilliseconds
            )
            |> Array.map caseNode
            |> Array.toList

        Xml.element "testsuite" [ Xml.attribute "name" suiteName ] cases
        |> fun suite -> Xml.element "testsuites" [] [ suite ]
        |> Xml.document

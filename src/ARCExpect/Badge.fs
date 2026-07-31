namespace ARCExpect.Badge

open ARCExpect
open Fable.Core

[<AttachMembers>]
type Color(hexCode: string) =

    let _hexCode = if isNull hexCode then "" else hexCode

    member _.HexCode = _hexCode

    static member create(hexCode: string) = Color(hexCode)
    static member red() = Color.create("#E05D44")
    static member orange() = Color.create("#FFA500")
    static member green() = Color.create("#4C1")

    override this.Equals(other) =
        match other with
        | :? Color as color -> this.HexCode = color.HexCode
        | _ -> false

    override this.GetHashCode() = hash this.HexCode

[<AttachMembers>]
type Threshold(value: int, color: Color) =

    let _value = value
    let _color = color

    member _.Value = _value
    member _.Color = _color

    static member create(value: int, color: Color) =
        Threshold(value, color)

module private Text =

    let escapeXml(value: string) =
        if isNull value then
            ""
        else
            value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;")

    let approximateWidth(text: string) =
        let groups = [|
            "lij|' ", 0.4
            "![]fI.,:;/\\t", 0.5
            "`-(){}r\"", 0.6
            "*^zcsJkvxy", 0.7
            "aebdhnopqug#$L+<>=?_~FZT0123456789", 0.7
            "BSPEAKVXY&UwNRCHD", 0.7
            "QGOMm%W@", 1.0
        |]

        let mutable size = 0.0

        for character in text do
            for characters, percentage in groups do
                if characters.IndexOf(character) >= 0 then
                    size <- size + percentage * 10.0

        int size

    let formatAnchor(value: float) =
        if value = System.Math.Floor(value) then
            string (int value)
        else
            let whole = int (System.Math.Floor(value))
            $"{whole}.5"

module private Colors =

    let select value (thresholds: Threshold array) defaultColor =
        if isNull thresholds || thresholds.Length = 0 then
            defaultColor
        else
            let ordered = thresholds |> Array.sortBy (fun threshold -> threshold.Value)

            ordered
            |> Array.tryFind (fun threshold -> value < threshold.Value)
            |> Option.map (fun threshold -> threshold.Color)
            |> Option.defaultWith (fun () -> ordered[ordered.Length - 1].Color)

[<AttachMembers>]
type Writer private () =

    static member toSvg(
        summary: ValidationSummary,
        labelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Threshold array,
        ?DefaultColor: Color
    ) =
        let total = summary.Critical.Total + summary.NonCritical.Total
        let totalPassed = summary.Critical.Passed + summary.NonCritical.Passed
        let criticalErrors = summary.Critical.Failed + summary.Critical.Errored

        let value, suffix, color =
            if summary.Critical.HasFailures then
                criticalErrors, " Critical Errors", Color.red()
            else
                let thresholds =
                    defaultArg Thresholds [|
                        Threshold.create(0, Color.red())
                        Threshold.create(total / 2, Color.orange())
                        Threshold.create(total, Color.green())
                    |]

                let color =
                    Colors.select
                        totalPassed
                        thresholds
                        (defaultArg DefaultColor (Color.green()))

                totalPassed, defaultArg ValueSuffix $"/{total}", color

        let valueText = string value + suffix
        let labelWidth = Text.approximateWidth labelText + 10
        let valueWidth = Text.approximateWidth valueText + 10
        let badgeWidth = labelWidth + valueWidth
        let labelAnchor = float labelWidth / 2.0
        let valueAnchor = float labelWidth + float valueWidth / 2.0
        let label = Text.escapeXml labelText
        let displayedValue = Text.escapeXml valueText
        let maskId = "arc-validate-badge"

        let template = """<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" width="{{badge-width}}" height="20">
    <linearGradient id="b" x2="0" y2="100%">
        <stop offset="0" stop-color="#bbb" stop-opacity=".1"/>
        <stop offset="1" stop-opacity=".1"/>
    </linearGradient>
    <mask id="{{mask-id}}">
        <rect width="{{badge-width}}" height="20" rx="3" fill="#fff"/>
    </mask>
    <g mask="url(#{{mask-id}})">
        <path fill="#555" d="M0 0h{{label-width}}v20H0z"/>
        <path fill="{{color}}" d="M{{label-width}} 0h{{value-width}}v20H{{label-width}}z"/>
        <path fill="url(#b)" d="M0 0h{{badge-width}}v20H0z"/>
    </g>
    <g fill="#fff" text-anchor="middle" font-family="DejaVu Sans,Verdana,Geneva,sans-serif" font-size="11">
        <text x="{{label-shadow}}" y="15" fill="#010101" fill-opacity=".3">{{label}}</text>
        <text x="{{label-anchor}}" y="14">{{label}}</text>
    </g>
    <g fill="#fff" text-anchor="middle" font-family="DejaVu Sans,Verdana,Geneva,sans-serif" font-size="11">
        <text x="{{value-shadow}}" y="15" fill="#010101" fill-opacity=".3">{{value}}</text>
        <text x="{{value-anchor}}" y="14">{{value}}</text>
    </g>
</svg>
"""

        template
            .Replace("{{badge-width}}", string badgeWidth)
            .Replace("{{mask-id}}", maskId)
            .Replace("{{label-width}}", string labelWidth)
            .Replace("{{value-width}}", string valueWidth)
            .Replace("{{color}}", color.HexCode)
            .Replace("{{label-shadow}}", Text.formatAnchor(labelAnchor + 1.0))
            .Replace("{{label-anchor}}", Text.formatAnchor labelAnchor)
            .Replace("{{value-shadow}}", Text.formatAnchor(valueAnchor + 1.0))
            .Replace("{{value-anchor}}", Text.formatAnchor valueAnchor)
            .Replace("{{label}}", label)
            .Replace("{{value}}", displayedValue)

namespace ARCExpect

open System
open System.Collections
open System.Collections.Generic
open Thoth.Json.Core

[<RequireQualifiedAccess>]
module internal DotNetPayload =

    let private number (value: obj) =
        let number =
            Convert.ToDouble(
                value,
                Globalization.CultureInfo.InvariantCulture
            )

        if Double.IsNaN number || Double.IsInfinity number then
            invalidArg "Payload" "Payload numbers must be finite."

        Json.Number number

    let rec value (item: obj) =
        match item with
        | null -> Json.Null
        | :? Json as json -> json
        | :? string as text -> Json.String text
        | :? char as character -> Json.String(string character)
        | :? bool as boolean -> Json.Boolean boolean
        | :? sbyte as value -> number value
        | :? byte as value -> number value
        | :? int16 as value -> number value
        | :? uint16 as value -> number value
        | :? int as value -> number value
        | :? uint32 as value -> number value
        | :? int64 as value -> number value
        | :? uint64 as value -> number value
        | :? single as value -> number value
        | :? double as value -> number value
        | :? decimal as value -> number value
        | :? IDictionary<string, obj> as dictionary ->
            dictionary
            |> Seq.map (fun entry -> entry.Key, value entry.Value)
            |> Seq.toList
            |> Json.Object
        | :? IEnumerable as values ->
            values
            |> Seq.cast<obj>
            |> Seq.map value
            |> Seq.toList
            |> Json.Array
        | unsupported ->
            invalidArg
                "Payload"
                $"Unsupported payload value type: {unsupported.GetType().FullName}."

    let dictionary (payload: IDictionary<string, obj>) =
        value payload

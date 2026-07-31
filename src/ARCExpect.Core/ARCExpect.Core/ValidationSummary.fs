namespace ARCExpect

open System.Collections.Generic
open System.Text.Json
open Thoth.Json.Core

[<RequireQualifiedAccess>]
module internal PayloadConversion =

    let rec private fromElement(element: JsonElement) =
        match element.ValueKind with
        | JsonValueKind.Null
        | JsonValueKind.Undefined -> Json.Null
        | JsonValueKind.True -> Json.Boolean true
        | JsonValueKind.False -> Json.Boolean false
        | JsonValueKind.String -> Json.String(element.GetString())
        | JsonValueKind.Number -> Json.Number(element.GetDouble())
        | JsonValueKind.Array ->
            element.EnumerateArray()
            |> Seq.map fromElement
            |> Seq.toList
            |> Json.Array
        | JsonValueKind.Object ->
            element.EnumerateObject()
            |> Seq.map (fun property -> property.Name, fromElement property.Value)
            |> Seq.toList
            |> Json.Object
        | _ -> Json.Null

    let fromDictionary(payload: Dictionary<string, obj>) =
        payload
        |> JsonSerializer.SerializeToElement
        |> fromElement

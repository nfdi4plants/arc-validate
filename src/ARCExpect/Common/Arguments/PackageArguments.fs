namespace ARCExpect

open System
open System.Globalization
open Fable.Core
open ValidationPackage.Model

type private PackageInputValue =
    | BooleanValue of bool
    | IntValue of int
    | LongValue of int64
    | FloatValue of float32
    | DoubleValue of float
    | StringValue of string

type private InputDefinition =
    {
        Id: string
        InputType: CommandInputType
        Binding: CommandInputBinding
    }

[<RequireQualifiedAccess>]
module private PackageArgumentParsing =

    let private standardArguments =
        [|
            "-i", "arc-directory"
            "--arc-directory", "arc-directory"
            "-o", "out-directory"
            "--out-directory", "out-directory"
            "--source-branch", "source-branch"
            "--source-commit-hash", "source-commit-hash"
        |]

    let private reservedPrefixes =
        standardArguments
        |> Array.map fst
        |> Array.append [| "--" |]
        |> Set.ofArray

    let private fail message = invalidArg "arguments" message

    let private isAsciiDigit value = value >= '0' && value <= '9'

    let private isIntegerLiteral (value: string) =
        if String.IsNullOrEmpty value then
            false
        else
            let startIndex =
                if value[0] = '+' || value[0] = '-' then 1 else 0

            startIndex < value.Length
            && value[startIndex..] |> Seq.forall isAsciiDigit

    let private consumeAsciiDigits (value: string) startIndex =
        let mutable index = startIndex

        while index < value.Length && isAsciiDigit value[index] do
            index <- index + 1

        index

    let private isFloatingPointLiteral (value: string) =
        if String.IsNullOrEmpty value then
            false
        else
            let mutable index =
                if value[0] = '+' || value[0] = '-' then 1 else 0

            let integerEnd = consumeAsciiDigits value index
            let hasIntegerDigits = integerEnd > index
            index <- integerEnd
            let mutable hasFractionDigits = false

            if index < value.Length && value[index] = '.' then
                index <- index + 1
                let fractionEnd = consumeAsciiDigits value index
                hasFractionDigits <- fractionEnd > index
                index <- fractionEnd

            let hasMantissaDigits = hasIntegerDigits || hasFractionDigits

            let hasValidExponent =
                if index < value.Length && (value[index] = 'e' || value[index] = 'E') then
                    index <- index + 1

                    if index < value.Length && (value[index] = '+' || value[index] = '-') then
                        index <- index + 1

                    let exponentEnd = consumeAsciiDigits value index
                    let hasExponentDigits = exponentEnd > index
                    index <- exponentEnd
                    hasExponentDigits
                else
                    true

            hasMantissaDigits && hasValidExponent && index = value.Length

    let private parseInputValue (definition: InputDefinition) (value: string) =
        let invalid expected =
            fail $"Input '{definition.Id}' expects a CWL {expected} value, but received '{value}'."

        match definition.InputType.PrimitiveType with
        | CwlPrimitive.Boolean ->
            match Boolean.TryParse value with
            | true, parsed -> BooleanValue parsed
            | false, _ -> invalid "boolean"
        | CwlPrimitive.Int ->
            match
                isIntegerLiteral value,
                Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture)
            with
            | true, (true, parsed) -> IntValue parsed
            | _ -> invalid "int"
        | CwlPrimitive.Long ->
            match
                isIntegerLiteral value,
                Int64.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture)
            with
            | true, (true, parsed) -> LongValue parsed
            | _ -> invalid "long"
        | CwlPrimitive.Float ->
            match
                isFloatingPointLiteral value,
                Single.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture)
            with
            | true, (true, parsed) -> FloatValue parsed
            | _ -> invalid "float"
        | CwlPrimitive.Double ->
            match
                isFloatingPointLiteral value,
                Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture)
            with
            | true, (true, parsed) -> DoubleValue parsed
            | _ -> invalid "double"
        | CwlPrimitive.String -> StringValue value
        | primitive -> fail $"Input '{definition.Id}' uses unsupported CWL primitive '{primitive}'."

    let private definitions (metadata: ValidationPackageMetadata) =
        if isNull (box metadata) then
            nullArg "metadata"

        let inputs =
            if isNull metadata.Inputs then Array.empty else metadata.Inputs

        let mutable ids = Set.empty
        let mutable prefixes = Set.empty
        let mutable positions = Set.empty

        inputs
        |> Array.map (fun input ->
            if isNull (box input) then
                fail "Validation-package metadata contains a null command input."

            if String.IsNullOrWhiteSpace input.Id then
                fail "Every validation-package command input must have a non-empty id."

            if Set.contains input.Id ids then
                fail $"Validation-package command input id '{input.Id}' is declared more than once."

            ids <- Set.add input.Id ids

            if isNull (box input.Type) then
                fail $"Input '{input.Id}' has no CWL type."

            if isNull (box input.InputBinding) then
                fail $"Input '{input.Id}' has no CWL inputBinding."

            let prefix = input.InputBinding.Prefix

            if not (isNull prefix) && prefix.Length > 0 then
                if String.IsNullOrWhiteSpace prefix then
                    fail $"Input '{input.Id}' has a whitespace-only command prefix."

                if Set.contains prefix reservedPrefixes then
                    fail $"Input '{input.Id}' uses reserved command prefix '{prefix}'."

                if Set.contains prefix prefixes then
                    fail $"Command prefix '{prefix}' is declared more than once."

                prefixes <- Set.add prefix prefixes
            else
                if Set.contains input.InputBinding.Position positions then
                    fail $"Positional command input position {input.InputBinding.Position} is declared more than once."

                positions <- Set.add input.InputBinding.Position positions

            {
                Id = input.Id
                InputType = input.Type
                Binding = input.InputBinding
            }
        )

    let parse (metadata: ValidationPackageMetadata) (arguments: string array) =
        if isNull arguments then
            nullArg "arguments"

        let definitions = definitions metadata

        let positionalDefinitions =
            definitions
            |> Array.filter (fun definition -> String.IsNullOrEmpty definition.Binding.Prefix)
            |> Array.sortBy (fun definition -> definition.Binding.Position)

        let prefixedDefinitions =
            definitions
            |> Array.filter (fun definition -> not (String.IsNullOrEmpty definition.Binding.Prefix))

        let joinedDefinitions =
            prefixedDefinitions
            |> Array.filter (fun definition -> not definition.Binding.Separate)
            |> Array.sortByDescending (fun definition -> definition.Binding.Prefix.Length)

        let mutable standardValues = Map.empty<string, string>
        let mutable inputValues = Map.empty<string, PackageInputValue>
        let mutable positionalIndex = 0
        let mutable index = 0

        let addStandard key value =
            if Map.containsKey key standardValues then
                fail $"Standard package argument '{key}' was supplied more than once."

            if String.IsNullOrWhiteSpace value then
                fail $"Standard package argument '{key}' requires a non-empty value."

            standardValues <- Map.add key value standardValues

        let addInput definition value =
            if Map.containsKey definition.Id inputValues then
                fail $"Package input '{definition.Id}' was supplied more than once."

            inputValues <-
                Map.add definition.Id (parseInputValue definition value) inputValues

        while index < arguments.Length do
            let token = arguments[index]

            match standardArguments |> Array.tryFind (fun (prefix, _) -> prefix = token) with
            | Some (_, key) ->
                if index + 1 >= arguments.Length then
                    fail $"Standard package argument '{token}' requires a value."

                addStandard key arguments[index + 1]
                index <- index + 2
            | None ->
                match prefixedDefinitions |> Array.tryFind (fun definition -> definition.Binding.Prefix = token) with
                | Some definition when definition.InputType.PrimitiveType = CwlPrimitive.Boolean ->
                    addInput definition "true"
                    index <- index + 1
                | Some definition when definition.Binding.Separate ->
                    if index + 1 >= arguments.Length then
                        fail $"Package input '{definition.Id}' requires a value after '{token}'."

                    addInput definition arguments[index + 1]
                    index <- index + 2
                | Some definition ->
                    fail $"Package input '{definition.Id}' requires its value to be joined to prefix '{token}'."
                | None ->
                    match
                        joinedDefinitions
                        |> Array.tryFind (fun definition ->
                            token.Length > definition.Binding.Prefix.Length
                            && token.StartsWith(definition.Binding.Prefix, StringComparison.Ordinal)
                        )
                    with
                    | Some definition ->
                        addInput
                            definition
                            (token.Substring(definition.Binding.Prefix.Length))
                        index <- index + 1
                    | None when positionalIndex < positionalDefinitions.Length ->
                        let definition = positionalDefinitions[positionalIndex]
                        addInput definition token
                        positionalIndex <- positionalIndex + 1
                        index <- index + 1
                    | None ->
                        fail $"Unknown validation-package argument '{token}'."

        for definition in definitions do
            if
                not (Map.containsKey definition.Id inputValues)
                && not definition.InputType.IsNullable
            then
                match definition.InputType.PrimitiveType with
                | CwlPrimitive.Boolean ->
                    inputValues <- Map.add definition.Id (BooleanValue false) inputValues
                | _ -> fail $"Required package input '{definition.Id}' was not supplied."

        let requiredStandard name =
            match Map.tryFind name standardValues with
            | Some value -> value
            | None -> fail $"Required standard package argument '{name}' was not supplied."

        requiredStandard "arc-directory",
        requiredStandard "out-directory",
        Map.tryFind "source-branch" standardValues,
        Map.tryFind "source-commit-hash" standardValues,
        inputValues

[<AttachMembers>]
type PackageArguments private (
    arcDirectory: string,
    outputDirectory: string,
    sourceBranch: string option,
    sourceCommitHash: string option,
    inputValues: Map<string, PackageInputValue>
) =

    let _arcDirectory = arcDirectory
    let _outputDirectory = outputDirectory
    let _sourceBranch = sourceBranch
    let _sourceCommitHash = sourceCommitHash
    let _inputValues = inputValues

    member _.ArcDirectory = _arcDirectory
    member _.OutputDirectory = _outputDirectory
    member _.SourceBranch = _sourceBranch
    member _.SourceCommitHash = _sourceCommitHash

    member _.InputIds =
        _inputValues |> Map.toArray |> Array.map fst

    member _.HasValue(id: string) =
        Map.containsKey id _inputValues

    member private _.GetValue(id: string) =
        match Map.tryFind id _inputValues with
        | Some value -> value
        | None -> invalidArg "id" $"Package input '{id}' was not supplied."

    member this.GetBoolean(id: string) =
        match this.GetValue id with
        | BooleanValue value -> value
        | _ -> invalidArg "id" $"Package input '{id}' is not a boolean."

    member this.TryGetBoolean(id: string) =
        if this.HasValue id then Some(this.GetBoolean id) else None

    member this.GetInt(id: string) =
        match this.GetValue id with
        | IntValue value -> value
        | _ -> invalidArg "id" $"Package input '{id}' is not an int."

    member this.TryGetInt(id: string) =
        if this.HasValue id then Some(this.GetInt id) else None

    member this.GetLong(id: string) =
        match this.GetValue id with
        | LongValue value -> value
        | _ -> invalidArg "id" $"Package input '{id}' is not a long."

    member this.TryGetLong(id: string) =
        if this.HasValue id then Some(this.GetLong id) else None

    member this.GetFloat(id: string) =
        match this.GetValue id with
        | FloatValue value -> value
        | _ -> invalidArg "id" $"Package input '{id}' is not a float."

    member this.TryGetFloat(id: string) =
        if this.HasValue id then Some(this.GetFloat id) else None

    member this.GetDouble(id: string) =
        match this.GetValue id with
        | DoubleValue value -> value
        | _ -> invalidArg "id" $"Package input '{id}' is not a double."

    member this.TryGetDouble(id: string) =
        if this.HasValue id then Some(this.GetDouble id) else None

    member this.GetString(id: string) =
        match this.GetValue id with
        | StringValue value -> value
        | _ -> invalidArg "id" $"Package input '{id}' is not a string."

    member this.TryGetString(id: string) =
        if this.HasValue id then Some(this.GetString id) else None

    static member parse(
        metadata: ValidationPackageMetadata,
        arguments: string array
    ) =
        let arcDirectory, outputDirectory, sourceBranch, sourceCommitHash, inputValues =
            PackageArgumentParsing.parse metadata arguments

        PackageArguments(
            arcDirectory,
            outputDirectory,
            sourceBranch,
            sourceCommitHash,
            inputValues
        )

    static member fromCommandLine(metadata: ValidationPackageMetadata) =
        PackageArguments.parse(metadata, TargetCommandLine.arguments())

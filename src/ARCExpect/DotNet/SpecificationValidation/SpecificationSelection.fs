namespace ARCExpect.SpecificationValidation

module SpecificationSelection =

    open ARCExpect

    let [<Literal>] latest = "2.0.0-draft"

    let internal specMap =
        [
            "2.0.0-draft" , SpecificationValidation.V2_0_0_Draft.validationCases
        ] |> Map.ofSeq

    let tryGetValidationCasesForSpecificationVersion (specVersion: string) (path: string) =

        let specVersion = if specVersion = "latest" then latest else specVersion

        match specMap.TryFind specVersion with
        | Some generator -> specVersion, generator path
        | None -> failwithf "No validation cases found for specification version '%s'" specVersion
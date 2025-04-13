namespace ARCExpect.SpecificationValidation


module SpecificationSelection =

    open ARCExpect

    let [<Literal>] latest = "2.1.0"

    let internal specMap =
        [
            "2.1.0" , SpecificationValidation.V2_1_0.getValidationCases
        ] |> Map.ofSeq

    let tryGetValidationCasesForSpecificationVersion (specVersion: string) (path: string) =

        let specVersion = if specVersion = "latest" then latest else specVersion

        match specMap.TryFind specVersion with
        | Some generator -> specVersion, generator path
        | None -> failwithf "No validation cases found for specification version '%s'" specVersion
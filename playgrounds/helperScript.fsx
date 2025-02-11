let tripleSlashGenerator className roCrateName isaName =
    printfn $"/// Returns the {roCrateName} of the {className} if it exists. Else returns None. This corresponds to `{isaName}` in ISA."
    printfn $"/// Returns the {roCrateName} of the {className}. This corresponds to `{isaName}` in ISA."
    printfn $"/// Returns the {roCrateName} of the given {className} if it exists. Else returns None. This corresponds to `{isaName}` in ISA."
    printfn $"/// Returns the {roCrateName} of the given {className}. This corresponds to `{isaName}` in ISA."

tripleSlashGenerator "Assay" "variableMeasured" "measurementType"
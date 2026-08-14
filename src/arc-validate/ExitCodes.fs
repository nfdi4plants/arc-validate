namespace ARCValidate

/// Enum to model different exit codes to be returned by the cli tool.
[<Struct>]
type ExitCode =
    /// The program returned no errors.
    | Success = 0
    /// At least one of the critical tests performed on the ARC failed or errored.
    | CriticalTestFailure = 1
    /// Argu returned an argument parsing error.
    | ArgParseError = 2
    /// Misc internal errors happened.
    | InternalError = 3
    /// Validation configuration or execution-plan input is invalid.
    | ConfigurationError = 4
    /// Registry discovery or metadata preflight failed.
    | RegistryError = 5

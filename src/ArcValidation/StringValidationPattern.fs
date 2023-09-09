namespace ArcValidation

module StringValidationPattern =

    open System
    open System.Text.RegularExpressions
    
    let email = Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")



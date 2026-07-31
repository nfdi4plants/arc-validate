namespace ARCExpect
open Expecto

[<AutoOpen>]
module ARCExpect =

    /// <summary>
    /// Computation expression for creating ARC validation cases.
    /// </summary>
    /// <param name="id">id of the test. can either be a guid or a string. Must be unique in the given context.</param>
    let validationCase (id:TestID) = 
        TestCaseBuilderSp(id)

    /// <summary>
    /// Passes if one of the given actions passes, and fails if both fail.
    /// </summary>
    /// <param name="arcExpect1"></param>
    /// <param name="arcExpect2"></param>
    let either arcExpect1 arcExpect2 = 
        try
            arcExpect1 () 
        with
        | ex1 -> 
            try
                arcExpect2 () 
            with
            | ex2 ->                          
                Expecto.Tests.failtestNoStackf "%s or %s" ex1.Message ex2.Message

namespace ArcValidation 

module ErrorMessage =

    open FsSpreadsheet.CellReference
    open ArcGraphModel
    open ArcGraphModel.IO
    open CvTokenHelperFunctions


    type MessageKind =
        | FilesystemEntryKind
        | TextfileKind
        | XLSXFileKind

    /// Representation of a Message. Path is mandatory, Line and Position are optional. 
    ///
    /// For mere binary files and folders and all files listed below when their content does not matter, Line, Position, and Sheet shall always be None. MessageKind is `FilesystemEntryKind`.
    ///
    /// For textfiles when content matters, Line and Position shall lead to the first character in question. Sheet shall be None. MessageKind is `TextfileKind`.
    ///
    /// For XLSX files when content matters, Line and Position together shall lead to the cell in question. Sheet shall be the name of the concerning Worksheet. MessageKind is `XLSXFileKind`.
    type Message = {
        Path        : string
        Line        : int option
        Position    : int option
        Sheet       : string option
        Kind        : MessageKind
        }
        with

        /// Creates a message with the given path, and, optionally, line and position. 
        /// 
        /// For mere binary files and folders and all files listed below when their content does not matter, Line, Position, and Sheet shall always be None. MessageKind is `FilesystemEntryKind`.
        ///
        /// For textfiles when content matters, Line and Position shall lead to the first character in question. Sheet shall be None. MessageKind is `TextfileKind`.
        ///
        /// For XLSX files when content matters, Line and Position together shall lead to the cell in question. Sheet shall be the name of the concerning Worksheet. MessageKind is `XLSXFileKind`.
        static member create path line pos sheet kind = {Path = path; Line = line; Position = pos; Sheet = sheet; Kind = kind}


    module FilesystemEntry =

        /// Creates a Message for a FilesystemEntry from the given CvParam.
        let createFromCvParam cvParam =
            let path = CvParam.tryGetAttribute (CvTerm.getName Terms.filepath) cvParam |> Option.get |> Param.getValueAsString
            Message.create path None None None FilesystemEntryKind

        /// Creates a Message for a FilesystemEntry from the given CvContainer.
        let createFromCvContainer cvContainer =
            let path = CvContainer.tryGetAttribute (CvTerm.getName Terms.filepath) cvContainer |> Option.get |> Param.getValueAsString
            Message.create path None None None FilesystemEntryKind

        /// Creates a Message for a FilesystemEntry from the given filepath.
        let createFromFile filepath =
            Message.create filepath None None None FilesystemEntryKind


    module Textfile =

        /// Creates a Message for a Textfile from the given CvParam.
        let createFromCvParam cvParam =
            let defaultMsg = FilesystemEntry.createFromCvParam cvParam
            let line = CvParam.tryGetAttribute (CvTerm.getName Address.row) cvParam |> Option.get |> Param.getValueAsInt
            let pos = CvParam.tryGetAttribute (CvTerm.getName Address.column) cvParam |> Option.get |> Param.getValueAsInt
            {defaultMsg with
                Line        = Some line
                Position    = Some pos
                Kind        = TextfileKind
            }

        /// Creates a Message for a Textfile from the given CvContainer.
        let createFromCvContainer cvContainer =
            let defaultMsg = FilesystemEntry.createFromCvContainer cvContainer
            let line = CvContainer.getAttributeValueAsIntFromProperties (CvTerm.getName Address.row) cvContainer
            let pos = CvContainer.getAttributeValueAsIntFromProperties (CvTerm.getName Address.column) cvContainer
            {defaultMsg with
                Line        = Some line
                Position    = Some pos
                Kind        = TextfileKind
            }


    module XlsxFile =

        /// Creates a Message for an XLSX file from the given CvParam.
        let createFromCvParam cvParam =
            let defaultMsg = Textfile.createFromCvParam cvParam
            let sheet = CvParam.tryGetAttribute (CvTerm.getName Address.worksheet) cvParam |> Option.get |> Param.getValueAsString
            {defaultMsg with
                Sheet   = Some sheet
                Kind    = XLSXFileKind
            }

        /// Creates a Message for an XLSX file from the given CvContainer.
        let createFromCvContainer cvContainer =
            let defaultMsg = Textfile.createFromCvContainer cvContainer
            let sheet = CvContainer.tryGetAttribute (CvTerm.getName Address.worksheet) cvContainer |> Option.get |> Param.getValueAsString
            {defaultMsg with
                Sheet   = Some sheet
                Kind    = XLSXFileKind
            }


    /// First part of an Error message: Describes why something did not match the requirements.
    module FailStrings =

        module FilesystemEntry =

            /// Takes a Message and returns a string containing the information that a FilesystemEntry is not present.
            let isPresent message = $"Actual entity is not present: {message.Path} in Worksheet {message.Sheet.Value} at Cell: {uint message.Position.Value |> indexToColAdress}{message.Line.Value}"

            /// Takes 2 Messages and returns a string containing the information that neither FilesystemEntry is present.
            let isEitherPresent message1 message2 = $"Neither of the actual entities are present: {message1.Path}, {message2.Path}"

            /// Takes a Message and returns a string containing the information that a FilesystemEntry is not registered.
            let isRegistered message = $"Actual value is not registered: {message.Path}"

            /// Takes a Message and returns a string containing the information that a term in a FilesystemEntry is not valid.
            let isValidTerm message = $"Actual term is not valid: {message.Path}"

            //let isValidVersion message = $"Actual CWL version is below required version 1.2: {message.Path}"

            /// Takes a Message and returns a string containing the information that a FilesystemEntry is not reproducible.
            let isReproducible message = $"Actual entity is not reproducible: {message.Path}"


        module Textfile =

            /// Takes a Message and returns a string containing the information that an information in a Textfile is not present.
            let isPresent message = $"Actual entity is not present: {message.Path} at Line: {message.Line.Value}, Position: {message.Position.Value}"

            /// Takes 2 Messages and returns a string containing the information that an information in neither Textfiles is present.
            let isEitherPresent message1 message2 = $"Neither of the actual entities are present: {message1.Path} at Line: {message1.Line.Value}, Position: {message1.Position.Value}, {message2.Path} at Line: {message2.Line.Value}, Position: {message2.Position.Value}"

            /// Takes a Message and returns a string containing the information that an information in a Textfile is not registered.
            let isRegistered message = $"Actual value is not registered: {message.Path} at Line: {message.Line.Value}, Position: {message.Position.Value}"

            /// Takes a Message and returns a string containing the information that an information in a Textfile is not a valid term.
            let isValidTerm message = $"Actual term is not valid: {message.Path} at Line: {message.Line.Value}, Position: {message.Position.Value}"

            /// Takes a Message and returns a string containing the information that an information in a Textfile is not a valid CWL version.
            let isValidVersion message = $"Actual CWL version is below required version 1.2: {message.Path} at Line: {message.Line.Value}, Position: {message.Position.Value}"

            ///// Takes a Message and returns a string containing the information that a FilesystemEntry is not reproducible.
            //let isReproducible message = $"Actual entity is not reproducible: {message.Path}"


        module XLSXFile =

            /// Takes a Message and returns a string containing the information that an information in an XLSXFile is not present.
            let isPresent message = $"Actual entity is not present: {message.Path} in Worksheet {message.Sheet} at Cell: {uint message.Position.Value |> indexToColAdress}{message.Line.Value}"

            /// Takes 2 Messages and returns a string containing the information that neither FilesystemEntry is present.
            let isEitherPresent message1 message2 = $"Neither of the actual entities are present: {message1.Path} in Worksheet {message1.Sheet} at Cell: {uint message1.Position.Value |> indexToColAdress}{message1.Line.Value}, {message2.Path} in Worksheet {message2.Sheet} at Cell: {uint message2.Position.Value |> indexToColAdress}{message2.Line.Value}"

            /// Takes a Message and returns a string containing the information that a FilesystemEntry is not registered.
            let isRegistered message = $"Actual value is not registered: {message.Path} in Worksheet {message.Sheet} at Cell: {uint message.Position.Value |> indexToColAdress}{message.Line.Value}"

            /// Takes a Message and returns a string containing the information that a term in an XLSXFile is not valid.
            let isValidTerm message = $"Actual term is not valid: {message.Path} in Worksheet {message.Sheet} at Cell: {uint message.Position.Value |> indexToColAdress}{message.Line.Value}"

            //let isValidVersion message = $"Actual CWL version is below required version 1.2: {message.Path}"

            /// Takes a Message and returns a string containing the information that a FilesystemEntry is not reproducible.
            let isReproducible message = $"Actual entity is not reproducible: {message.Path} in Worksheet {message.Sheet} at Cell: {uint message.Position.Value |> indexToColAdress}{message.Line.Value}"
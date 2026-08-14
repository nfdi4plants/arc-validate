namespace ARCValidate.Configuration

open System
open System.IO
open System.Security.Cryptography
open System.Text
open ValidationPackage.Codecs

/// Reads validation-package configuration bytes without weakening the portable codec.
[<RequireQualifiedAccess>]
module ValidationConfigFile =

    let private strictUtf8 = UTF8Encoding(false, true)
    let private utf8Bom = [| 0xEFuy; 0xBBuy; 0xBFuy |]

    let private displayPath path =
        try Path.GetFullPath(path)
        with _ -> path

    let private fail path message =
        raise (ConfigurationException($"{displayPath path}: {message}"))

    let private hasBom (bytes: byte array) =
        bytes.Length >= utf8Bom.Length
        && bytes[0] = utf8Bom[0]
        && bytes[1] = utf8Bom[1]
        && bytes[2] = utf8Bom[2]

    /// Reads, hashes, strictly decodes, and parses one explicitly named configuration file.
    let load path =
        if String.IsNullOrWhiteSpace(path) then
            fail path "validation configuration path must be non-empty"

        let bytes =
            try File.ReadAllBytes(path)
            with
            | :? UnauthorizedAccessException as error -> fail path $"cannot be read: {error.Message}"
            | :? IOException as error -> fail path $"cannot be read: {error.Message}"

        let sha256 =
            SHA256.HashData(bytes)
            |> Convert.ToHexString
            |> _.ToLowerInvariant()

        let contentBytes =
            if hasBom bytes then bytes[utf8Bom.Length..] else bytes

        let yaml =
            try strictUtf8.GetString(contentBytes)
            with :? DecoderFallbackException as error ->
                fail path $"is not valid UTF-8: {error.Message}"

        let decoded =
            match ValidationPackagesConfigYaml.decode yaml with
            | Ok config -> config
            | Error message -> fail path message

        {
            Path = displayPath path
            Sha256 = sha256
            Decoded = decoded
        }

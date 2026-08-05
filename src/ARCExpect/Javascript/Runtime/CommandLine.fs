namespace ARCExpect

open Fable.Core

[<RequireQualifiedAccess>]
module internal TargetCommandLine =

    [<Emit("globalThis.process?.argv?.slice(2) ?? (() => { throw new Error('PackageArguments.fromCommandLine requires a Node process.'); })()")>]
    let arguments(): string array = nativeOnly

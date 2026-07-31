# ARCExpect.Core

`ARCExpect.Core` is the .NET compatibility boundary for ARC validation package
execution. It converts Expecto results into `ARCExpect.Core.Portable` contracts
and owns filesystem writes and other .NET-only integrations.

Use `ARCExpect.Core.Portable` directly when code must transpile with Fable.

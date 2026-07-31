# ARCExpect.Core.Portable

Transitional portable result and output contracts for ARC validation packages.

The package contains framework-neutral case outcomes, case results, run
summaries, validation summaries, and pure JSON, JUnit, and SVG badge writers.
It has no runner, filesystem, HTTP, or process-execution responsibilities.

- `ARCExpect.ValidationSummary` owns Thoth.Json summary encoding and decoding.
- `ARCExpect.JUnit.Writer` produces JUnit XML through the maintained portable
  XML encoder.
- `ARCExpect.Badge.Writer` produces dependency-free SVG badges.

The same F# sources are compiled for .NET and transpiled with Fable for
JavaScript and Python. The final project/package boundary may be consolidated
into `ARCExpect.Core` after the runner transition.

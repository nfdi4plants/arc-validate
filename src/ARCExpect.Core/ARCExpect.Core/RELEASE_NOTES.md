### 7.0.0-alpha.1 - TBD

- Replace AVPRIndex metadata and frontmatter dependencies with the portable
  ValidationPackage model and codecs.
- Move result, summary, JUnit, and badge contracts into ARCExpect.Core.Portable.
- Retain Expecto conversion and filesystem writes behind the .NET boundary.
- Remove AnyBadge.NET and delegate badge/JUnit generation to the maintained
  portable `ARCExpect.Badge` and `ARCExpect.JUnit` writers.

### 7.0.0-alpha - 2026-02-27

- Initial standalone ARCExpect.Core package.

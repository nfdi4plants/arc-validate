### 1.0.0 - tbd

- Absorb validation-package registry access, configuration, caching, installation,
  and script execution into the CLI under `PackageManagement` and `PackageRunner`.
- Honor configured cache folders and registry endpoints, write cache/config/package
  files atomically, classify registry failures by HTTP status, and preserve process
  arguments without FAKE runtime dependencies.
- Accept optional `--source-branch` and `--source-commit-hash` validation
  arguments and carry them into every generated validation output.
- Accept package-defined arguments after a strict `--` boundary and forward
  every token unchanged through argument-list process execution.
- Add `config resolve --validation-config` with exact SemVer roll-forward
  policy resolution, bounded registry metadata preflight, stable configuration
  and registry exit codes, and a strict `validation_plan.json` contract.
- Ship the standalone validation-plan JSON Schema in CLI, publish, container,
  and documentation output and calculate its config digest over exact UTF-8
  bytes, including a leading BOM.

### 1.0.0 - tbd

- Absorb validation-package registry access, configuration, caching, installation,
  and script execution into the CLI under `PackageManagement` and `PackageRunner`.
- Honor configured cache folders and registry endpoints, write cache/config/package
  files atomically, classify registry failures by HTTP status, and preserve process
  arguments without FAKE runtime dependencies.

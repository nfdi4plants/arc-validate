# Contributing

Every contribution is welcome, such as:

- Bug reports
- Feature requests
- Documentation improvement requests
- Typo fixes
- Performance discussions and improvements
- New algorithm implementations
- etc.

**Please start by opening an issue**.

**Check the [Development section](https://github.com/nfdi4plants/arc-validate/tree/release#development)** for general guidance on the codebase.

**Pull Requests should target the `dev` branch from a forked version of the repo.**

This is an **open source project** created as the result of scientific teaching and research efforts.
Please refrain from unrealistic expectations from maintainers.

### Build

Just call `build.sh` or `build.cmd` depending on your OS.

### Test

```bash
build.sh runtests
```

```bash
build.cmd runtests
```

### Build ARCExpect packages

```bash
build.sh PackARCExpect
```

```bash
build.cmd PackARCExpect
```

The version, including any prerelease suffix, comes from
`ARCExpectPackageVersion` in `Directory.Build.props`. Package publication uses
the protected, manually dispatched GitHub Actions workflow documented in
[the release guide](docs/development/releases.md).

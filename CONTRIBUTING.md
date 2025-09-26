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

### Create Nuget package

```bash
build.sh pack
```

```bash
build.cmd pack
```

For prereleases use

```bash
build.sh packprerelease
```

```bash
build.cmd packprerelease
```

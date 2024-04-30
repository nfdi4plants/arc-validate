### 1.0.0 - (Released 2024-04-30)

Initial release of the ArcValidationPackages API.

This library provides a set of functions to retrieve, cache, manage, and execute validation packages for ARCs.

2 package sources are supported: 

- `Preview` packages are pulled from the `avpr-preview-index.json` file attached to the [Latest preview index release of the AVPR](https://github.com/nfdi4plants/arc-validate-package-registry/releases/tag/preview-index) 
- `AVPR` packages are pulled directly via the [AVPR API](https://avpr.nfdi4plants.org/swagger/index.html)

The difference between theses sources is that `Preview` packages are a snapshot of the AVPR staging area, where packages could still changed, while `AVPR` packages are the final versions of the packages and immutable.

Note that matching versions available from both sources should be the same, but no guarantee is given. Stable packages should always be consumed from the AVPR API.

Locally installed packages are managed in a directory (by default, `<SpecialFolder.ApplicationData>/nfdi4plants/arc-validate` in ``) containing the following files and folders:

containing these files:

```
<SpecialFolder.ApplicationData>/nfdi4plants/arc-validate
│   validation-packages-config.json
│ 
├───package-cache-preview
│       validation-packages-cache.json
│
└───package-cache-release
        validation-packages-cache.json
```

- validation-packages-config.json contains a local copy of the AVPR preview index and some config settings
- package-cache-preview contains `validation-packages-cache.json` file that contains metadata of installed preview validation packages, and the cached packages themselves
- package-cache-release contains `validation-packages-cache.json` file that contains metadata of installed AVPR validation packages, and the cached packages themselves

Installed validation packages can be executed via the `ScriptExecution` API, which uses FSI to execute the F# scripts.

A Top-level API using multiple integrated functions to perform common tasks is provided, consisting of the following classes:

- `Common` - Functionality that does not depend on the package source:
- `Preview` - Functionality for managing preview validation packages:
- `AVPR` - Functionality for managing AVPR validation packages:
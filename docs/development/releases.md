# Releasing ARCExpect

ARCExpect has one version and three distribution artifacts:

| Ecosystem | Package |
| --- | --- |
| NuGet | `ARCExpect` |
| npm | `@nfdi4plants/arcexpect` |
| PyPI | `arcexpect` |

`Directory.Build.props` is the authoritative version source. The first
versioned heading in `src/ARCExpect/RELEASE_NOTES.md` must contain the same
version and use `## <version> - YYYY-MM-DD`.

Prerelease notes use one rolling top entry: bump that entry in place and keep
accumulating preview changes beneath it. Published non-preview entries remain
immutable.

## Prepare and verify a release

1. Update `ARCExpectPackageVersion` in `Directory.Build.props`.
2. Add the matching release-notes entry.
3. Update exact sample and dependency pins when required.
4. Run:

   ```shell
   ./build.sh ValidateReleaseMetadata
   ./build.sh TestPortableARCExpect
   ```

   On Windows, use `build.cmd`.

5. Merge the reviewed change into `dev` for a preview publication, or into
   `release` for a production publication.
6. In GitHub Actions, manually run `Release ARCExpect` and select that branch.
7. Approve the protected `release` environment.

The workflow tests the selected revision, packs once, uploads those exact
artifacts, and then publishes them through independent registry jobs from the
protected environment. It is deliberately triggered by `workflow_dispatch`
without path filters. A failed run can be rerun directly, or the workflow can
be dispatched again for the same ref; no unrelated source change is needed to
re-enable publication. Prefer **Re-run failed jobs** after a partial registry
failure so successful registry jobs remain untouched.

Retries are safe after a partial publication: NuGet skips an existing version,
npm checks the exact version before publishing, and PyPI skips existing files.

## Trusted-publisher configuration

Create a GitHub environment named `release`, protect it with required
reviewers, and allow deployments from both `dev` and `release`. The same
trusted workflow can then be manually dispatched for a reviewed preview or a
production version. Store `NUGET_USER` there with the nuget.org profile that
owns the trusted-publishing policy (currently `Mutagene`). It is not an email address,
GitHub username, organization name, or API key. Do not create npm or PyPI token
secrets.

Configure the following publishers:

| Registry | Publisher fields |
| --- | --- |
| NuGet | Policy owner `Mutagene`; repository owner `nfdi4plants`; repository `arc-validate`; workflow `release-arcexpect.yml`; environment `release` |
| npm | Package `@nfdi4plants/arcexpect`; GitHub organization/user `nfdi4plants`; repository `arc-validate`; workflow `release-arcexpect.yml`; environment `release`; allowed action `npm publish` |
| PyPI | Project `arcexpect`; GitHub owner `nfdi4plants`; repository `arc-validate`; workflow `release-arcexpect.yml`; environment `release` |

Enter workflow filenames only, without `.github/workflows/`. The npm values
are case-sensitive. The npm package's repository URL must remain
`https://github.com/nfdi4plants/arc-validate.git`.

NuGet trusted-publishing policies belong to a NuGet user or organization and
cannot be scoped to an individual package. With `Mutagene` as policy owner, the
policy can technically publish any NuGet package owned by that account; the
workflow restriction does not create a package-level boundary. Keep ARCExpect
owned by `Mutagene` for this configuration.

See the registry documentation for
[NuGet trusted publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing),
[npm trusted publishers](https://docs.npmjs.com/trusted-publishers/), and
[PyPI trusted publishers](https://docs.pypi.org/trusted-publishers/adding-a-publisher/).

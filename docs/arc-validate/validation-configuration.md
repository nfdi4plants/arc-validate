# Resolve validation configuration

`arc-validate config resolve` turns an ARC's explicit
`.arc/validation_packages.yml` into a preflighted `validation_plan.json`.
The registry package index determines eligible versions, and exact metadata
requests validate every package declaration and configured input before the
command emits a plan.

```bash
arc-validate config resolve \
  --validation-config .arc/validation_packages.yml \
  > validation_plan.json
```

The canonical persisted filename is `validation_plan.json`. Standard output
contains only the complete JSON document; diagnostics and the one-time legacy
format warning use standard error. A configuration failure exits with code 4,
and a registry discovery or response failure exits with code 5.

The plan records the exact configuration byte digest and resolved identities,
while package input values remain in the source configuration and never enter
the CI-facing artifact:

```json
{
  "$schema": "https://nfdi4plants.github.io/arc-validate/schemas/v1/validation_plan.schema.json",
  "config_sha256": "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
  "arc_specification": "3.0.0-draft.2",
  "validation_packages": [
    {
      "name": "configurable-validation",
      "requested_version": "1.2.3",
      "roll_forward": "latest_patch",
      "resolved_version": "1.2.7"
    }
  ]
}
```

The standalone [validation-plan JSON Schema](https://nfdi4plants.github.io/arc-validate/schemas/v1/validation_plan.schema.json)
is the authoritative structural contract. Runtime readers dispatch only on its
exact `$schema` URI and do not fetch schemas from the network.

## Execute a resolved package selection

Each child job uses the exact package identity and configuration digest recorded
by the parent resolver:

```bash
arc-validate validate \
  --arc-directory . \
  --package configurable-validation \
  --package-version 1.2.7 \
  --validation-config .arc/validation_packages.yml \
  --validation-config-sha256 0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef
```

The child hashes the file's exact bytes (including a UTF-8 BOM), verifies the
optional lowercase digest before using any values, and rechecks that `1.2.7`
satisfies the source selection. It then loads only that exact installed cache
entry and validates the configured values against that version's metadata. It
does not query AVPR or ask whether a newer version has appeared since the plan
was created.

Direct interactive calls may omit `--validation-config-sha256`. Generated
DataHUB jobs must pass it so parent resolution and child execution are bound to
the same bytes. Config-driven mode requires `--package`, `--package-version`,
and an explicit `--validation-config` path together, and cannot be combined
with raw package arguments after `--`.

Configured values become separate process arguments in declaration position
and ordinal ID order. A true boolean emits only its prefix; false and null emit
nothing; every other value emits a prefix token followed by one unchanged value
token. Manual package arguments remain available when configured mode is not
selected.

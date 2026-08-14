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

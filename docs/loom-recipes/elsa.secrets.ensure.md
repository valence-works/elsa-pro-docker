# Loom Recipe Step: elsa.secrets.ensure

## Status

Initial locked draft for the first Elsa Pro Server Loom recipe slice.

## Purpose

Define a logical Elsa secret and bind it to a registered secret store.

This step gives recipes a portable way to refer to secrets without carrying secret material directly in identity, persistence, messaging, or integration steps. A logical secret name can point to environment variables for Docker quick start, local/generated secrets for development, or external stores such as Docker secrets, Kubernetes secrets, Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault.

## Step Type

`elsa.secrets.ensure`

## Step Shape

Typed step:

```csharp
[Step("elsa.secrets.ensure")]
public sealed class EnsureSecretStep : IStep<EnsureSecretOutput>, IValidatingStep
```

## Conceptual Model

The step defines this relationship:

```text
logical secret name -> registered secret store binding
```

The logical `Name` is what other Elsa recipe steps use. The `Store`, `Key`, and `Parameters` describe where and how the secret is resolved.

Logical names are unique within a running Elsa Pro Server secret registry. Shell, tenant, or environment scope should be encoded in the logical name when needed, for example `elsa/default/admin-password` or `elsa/tenant-a/admin-password`.

The step must not make downstream recipe steps know whether a secret comes from an environment variable, local store, cloud vault, Docker secret, Kubernetes secret, or generated value.

`Parameters` are provider-specific lookup details that belong to one secret binding, not to the store registration. For example, an Azure Key Vault binding might use `parameters.version` to pin a specific secret version, while an AWS Secrets Manager binding might use `parameters.jsonPath` to select a field from a JSON secret. The v1 `environment` and `local` stores do not require parameters unless explicitly documented by the store.

## Input Properties

```csharp
public string Name { get; set; } = "";
public string Store { get; set; } = "";
public string Key { get; set; } = "";
public Dictionary<string, object?> Parameters { get; set; } = [];
public SecretValueInput? Value { get; set; }
public SecretGenerationInput? Generate { get; set; }
public bool OverwriteExistingValue { get; set; }
```

```csharp
public sealed class SecretValueInput
{
    public string Value { get; set; } = "";
}

public sealed class SecretGenerationInput
{
    public string Kind { get; set; } = "base64url";
    public int Bytes { get; set; } = 32;
}
```

## Output Shape

```csharp
public sealed record EnsureSecretOutput(
    string Name,
    string Store,
    string Key,
    bool Changed,
    string Mode);
```

`Mode` should be one of:

- `reference`
- `value`
- `generated`
- `existing`

The output must not include secret material.

## Store Model

`Store` should identify a registered secret store instance, not just a provider type. For example:

```json
{
  "Elsa": {
    "Secrets": {
      "Stores": {
        "environment": {
          "Type": "Environment"
        },
        "prod-key-vault": {
          "Type": "AzureKeyVault",
          "VaultUri": "https://my-vault.vault.azure.net/"
        }
      }
    }
  }
}
```

This lets recipe definitions stay concise:

```json
{
  "name": "elsa/prod/admin-password",
  "store": "prod-key-vault",
  "key": "elsa-prod-admin-password"
}
```

The smallest useful implementation should ship two store instances:

- `environment`: reference-only. `Key` is the environment variable name and values are resolved from the process environment when consumers need them.
- `local`: writable. Used for development, bootstrap, and generated secrets. Encryption at rest is a separate concern and should be made explicit in documentation.

The logical secret registry and provider-side secret values must remain separate:

- The logical registry stores only `Name -> Store + Key + Parameters`.
- The `local` store stores secret values by `Key`.
- The logical registry must not contain secret material, even for local/generated secrets.

## Supported Modes

Reference existing provider secret:

```json
{
  "name": "elsa/default/admin-password",
  "store": "environment",
  "key": "ELSA_ADMIN_PASSWORD"
}
```

Write literal value to a writable store:

```json
{
  "name": "elsa/dev/admin-password",
  "store": "local",
  "key": "elsa/dev/admin-password",
  "value": {
    "value": "ChangeThisPassword123!"
  }
}
```

Generate value into a writable store:

```json
{
  "name": "elsa/default/identity-signing-key",
  "store": "local",
  "key": "elsa/default/identity-signing-key",
  "generate": {
    "kind": "base64url",
    "bytes": 32
  }
}
```

Literal values are allowed for development and bootstrap scenarios, but docs should discourage committing recipes containing real secret material.

## Validation Rules

- `Name` is required.
- `Name` must be unique within the Elsa Pro Server secret registry.
- `Store` is required.
- `Store` must resolve to a registered secret store instance.
- `Key` is required.
- Secret-name syntax is not globally constrained by this step beyond being non-empty; provider/store-specific rules validate `Key` and `Parameters`.
- Exactly one of these modes must be selected when the logical secret binding does not already exist:
  - reference existing provider secret: `Key` only.
  - write literal value: `Key` plus `Value`.
  - generate value: `Key` plus `Generate`.
- `Value` and `Generate` are mutually exclusive.
- `Value` requires a writable store.
- `Generate` requires a writable store.
- `Generate.Kind` must be supported by the store or generator. First implementation should support `base64url`.
- `Generate.Bytes` must be positive. Identity signing keys should use at least `32` bytes.
- `OverwriteExistingValue` only applies to writable stores and only controls provider-side secret values, not logical binding reconciliation.
- The selected store validates whether the binding shape is acceptable for that store.
- In reference mode, this step does not fail validation merely because the provider-side secret value is not currently present or readable. Consumers fail later when they actually need to resolve the secret value.
- Stores may emit warnings when a referenced value is not currently available, but those warnings are not validation failures for this step.

## Idempotency

The step is idempotent:

- If the logical secret binding already exists and matches the requested store/key/parameters, no binding change is made.
- If the logical secret binding exists but points somewhere else, it is updated to the requested store/key/parameters.
- In reference mode, the provider secret value is not copied, read as a requirement, or modified.
- In value mode, the step writes the value only when the target store has no value or `OverwriteExistingValue` is `true`.
- In generate mode, the step generates and writes a value only when the target store has no value or `OverwriteExistingValue` is `true`.
- Re-running a generate-mode recipe must not generate a new value when the target already exists and `OverwriteExistingValue` is `false`.

## Required Services

Initial implementation services:

- Secret registry that maps logical `Name` values to store bindings.
- Secret store registry that resolves registered store instances by `Store`.
- Secret resolver/reader.
- Secret writer for writable stores.
- Secret generator for generated modes.
- `ILogger<EnsureSecretStep>`.

Potential shared helper:

- `IElsaSecretRegistry` for logical-name bindings.
- `ISecretStoreRegistry` for store instance lookup.

## Recipe Examples

Docker environment variable backed admin password:

```json
{
  "type": "elsa.secrets.ensure",
  "id": "admin-password-secret",
  "input": {
    "name": "elsa/default/admin-password",
    "store": "environment",
    "key": "ELSA_ADMIN_PASSWORD"
  }
}
```

Generated local signing key:

```json
{
  "type": "elsa.secrets.ensure",
  "id": "identity-signing-key-secret",
  "input": {
    "name": "elsa/default/identity-signing-key",
    "store": "local",
    "key": "elsa/default/identity-signing-key",
    "generate": {
      "kind": "base64url",
      "bytes": 32
    }
  }
}
```

Using the outputs in later steps:

```json
{
  "type": "elsa.identity.admin.ensure",
  "id": "admin",
  "input": {
    "username": "admin",
    "passwordSecretName": "[js: output('admin-password-secret', 'name')]",
    "roleName": "admin",
    "permissions": ["*"]
  }
}
```

## Relationship To Other Steps

This is the foundational secret primitive for:

- `elsa.identity.signing-key.ensure`
- `elsa.identity.admin.ensure`
- future persistence connection-string steps
- future messaging connection-string steps
- future integration credential steps

Identity and infrastructure steps should consume logical secret names instead of accepting raw secret values where possible.

## Open Questions

- What should the v1 persisted shape of the logical secret registry be?
- Should `Value` be allowed in production mode, or should the runner provide a policy hook to reject literal secrets?
- Should generated local secrets be encrypted at rest, and if so, what key protects them?

## Deferred

- Interactive prompting.
- Secret rotation workflows.
- Versioned secrets.
- Secret leases and expiry.
- Provider-specific advanced options beyond `Parameters`.

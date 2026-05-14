# Loom Recipe Step: elsa.identity.signing-key.ensure

## Status

Initial locked draft for the first Elsa Pro Server Loom recipe slice.

## Purpose

Ensure a shell has a usable identity signing key reference for Elsa Identity tokens.

This step addresses one of the most important first-run production hazards in Elsa Pro Docker: the default config contains an empty or placeholder signing key. The step should make that state explicit, validate the referenced secret, and configure the shell to use that logical secret.

## Step Type

`elsa.identity.signing-key.ensure`

## Step Shape

Typed step:

```csharp
[Step("elsa.identity.signing-key.ensure")]
public sealed class EnsureIdentitySigningKeyStep : IStep<EnsureIdentitySigningKeyOutput>, IValidatingStep
```

## Input Properties

```csharp
public string SecretName { get; set; } = "";
public int MinimumBits { get; set; } = 256;
public bool ReplacePlaceholder { get; set; } = true;
```

## Output Shape

```csharp
public sealed record EnsureIdentitySigningKeyOutput(
    bool Changed,
    string SecretName,
    string KeySource);
```

`KeySource` should be one of:

- `existing`
- `placeholder-replaced`

The step must not accept or output raw signing key values. Recipes should pass a secret name only. The secret can be defined by `elsa.secrets.ensure` or by any compatible secret-management feature that resolves logical secret names at runtime.

## Runtime Target

The logical target is the shell's identity token signing settings. The preferred implementation should write through an Elsa Pro configuration/options abstraction that owns the persisted setting and any .NET configuration change notification.

This is a shell-scoped step. The target shell is not an input property. It is resolved from Loom execution context, using the Elsa convention key `elsa.shell`. Loom itself does not interpret this key; Elsa/CShells middleware should read it and establish the correct shell scope before validation and execution.

The step must not make recipe authors care about the underlying storage shape. For the first implementation, the concrete persistence mechanism may be a config patch that the secret-management feature later binds into runtime options. The step should not mutate `IOptions<T>.Value` directly as its primary persistence mechanism.

The step should not directly reload shells. If the backing configuration API can idiomatically notify .NET configuration change tokens after a write, that notification can happen as part of the configuration abstraction. Whether that notification causes shell reloads is host/runner policy and should not be hard-coded into this step.

Expected config representation for v1:

```json
{
  "CShells": {
    "Shells": {
      "{ShellName}": {
        "Features": {
          "Identity": {
            "SigningKeySecretName": "..."
          }
        }
      }
    }
  }
}
```

## Validation Rules

- The Loom execution context must contain `elsa.shell`.
- `SecretName` is required.
- `MinimumBits` must be at least `256`.
- The referenced logical secret must exist.
- If the referenced secret exists, its value must satisfy `MinimumBits`.
- Secret-name syntax is validated by the configured secret-management provider, not by this step. The step only requires a non-empty `SecretName`.
- Known placeholder values are invalid when they would remain in effect:
  - empty string
  - `CHANGE_ME_TO_A_SECURE_RANDOM_KEY`
  - `CHANGE_ME_TO_A_SECURE_RANDOM_KEY_AT_LEAST_256_BITS`
  - values containing `CHANGE_ME`
- If the shell contains an existing non-placeholder inline signing key and does not already reference `SecretName`, validation should fail. Inline key migration must be explicit and belongs to a later migration or rotation step.
- If the shell does not exist, validation should fail for the first implementation. Creating shells belongs to `elsa.shell.ensure` or `elsa.shells.features.enable`.
- If the referenced logical secret is missing, validation should fail and suggest defining it first with `elsa.secrets.ensure`.

## Idempotency

The step is idempotent:

- If the shell already references `SecretName` and the referenced secret value is valid, no changes are made.
- If the shell contains a placeholder or empty inline signing key and `ReplacePlaceholder` is `true`, the step replaces it with the secret reference.
- If the shell contains a valid inline signing key and does not already reference `SecretName`, the step fails validation instead of guessing precedence between inline signing keys and secret references.

Secret creation, generation, import, and overwrite behavior belongs to `elsa.secrets.ensure`. This step only consumes an existing logical secret and wires identity settings to it.

## Required Services

Initial implementation services:

- `IConfiguration` or an Elsa Pro configuration document abstraction.
- A JSON configuration writer/merger for `/config/config.json` or another selected recipe target.
- Secret management service capable of resolving named secrets.
- Elsa Identity token settings/configuration accessor or updater, if available.
- `ILogger<EnsureIdentitySigningKeyStep>`.

Likely shared helper:

- `IElsaShellConfigurationStore` or similar abstraction to read and patch the context-selected shell without stringly JSON edits in every step.

## Recipe Example

The referenced secret can be created by `elsa.secrets.ensure`:

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

```json
{
  "id": "default-shell",
  "context": {
    "elsa.shell": "Default"
  },
  "steps": [
    {
      "type": "elsa.identity.signing-key.ensure",
      "id": "signing-key",
      "input": {
        "secretName": "[js: output('identity-signing-key-secret', 'name')]",
        "replacePlaceholder": true
      }
    }
  ]
}
```

With interpolated secret name:

```json
{
  "type": "elsa.identity.signing-key.ensure",
  "id": "signing-key",
  "input": {
    "secretName": "elsa/[js: variables('shell')]/identity-signing-key",
    "minimumBits": 256
  }
}
```

In normal recipes, this step should be placed inside a group whose context contains `elsa.shell`.

## Open Questions

- What exact options/configuration key should the Identity feature bind to for secret references? `SigningKeySecretName` is acceptable for v1, but the secret-management feature may finalize the runtime contract.
- Should validation estimate entropy or only enforce decoded byte length? First implementation should enforce decoded byte length conservatively and reject known placeholders.
- What recipe-level or host-level policy should control shell reload/apply behavior after configuration-changing steps?
- What final Loom JSON shape should represent grouped steps and inherited execution context?

## Deferred

- Secret store integration.
- Key rotation with grace periods.
- Per-environment secret policy.
- Emitting or accepting secret material in recipes.
- Inline signing-key migration into secret management.
- Secret creation, generation, and overwrite behavior.

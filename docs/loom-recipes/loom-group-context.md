# Loom Recipe Capability: Group Context

## Status

Draft companion concept for Elsa Pro Server recipe steps.

## Purpose

Allow recipes to group steps under arbitrary context values that Loom carries into validation and execution.

Elsa Pro recipe steps need a clean way to run several steps inside the same CShells shell without repeating `ShellName` on every step. Loom should stay generic: it does not interpret Elsa-specific keys, but it should make group context available to middleware and step handlers.

## Proposed JSON Shape

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
        "secretName": "elsa/default/identity-signing-key"
      }
    },
    {
      "type": "elsa.identity.admin.ensure",
      "id": "admin",
      "input": {
        "username": "admin",
        "passwordSecretName": "elsa/default/admin-password",
        "roleName": "admin",
        "permissions": ["*"]
      }
    }
  ]
}
```

## Semantics

- Groups can contain steps.
- Groups carry a `context` object.
- Context keys are arbitrary and are not interpreted by Loom.
- Context is inherited by child steps.
- Inner group context overrides outer group context for matching keys.
- Middleware can read context before validation and execution.
- Typed steps can read context through `StepContext` and `StepValidationContext`.
- Elsa/CShells middleware should use the `elsa.shell` context key to establish a shell scope for shell-scoped steps.

## Elsa Convention

`elsa.shell` identifies the target CShells shell for shell-scoped Elsa recipe steps.

Shell-scoped steps should not expose `ShellName` input properties. They should require the context key instead.

## Open Questions

- What exact JSON shape should Loom use for groups in full recipes?
- Should groups be nestable in v1?
- Should context values support interpolation?
- Should context appear in step output or only be available to middleware and step code?

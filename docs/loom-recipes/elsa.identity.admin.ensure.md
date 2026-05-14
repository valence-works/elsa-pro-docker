# Loom Recipe Step: elsa.identity.admin.ensure

## Status

Initial locked draft for the first Elsa Pro Server Loom recipe slice.

## Purpose

Ensure an administrator role and user exist for an Elsa shell.

This step should cover the practical first-run need of being able to sign into Elsa Studio with an administrative account, while staying idempotent and avoiding environment-variable-only provisioning. Recipes must reference the password by secret name instead of carrying raw password material. The secret can be defined by `elsa.secrets.ensure` or by any compatible secret-management feature that resolves logical secret names at runtime.

The initial step manages one user and one role per execution. Recipes can repeat this step for additional administrative users.

## Step Type

`elsa.identity.admin.ensure`

## Step Shape

Typed step:

```csharp
[Step("elsa.identity.admin.ensure")]
public sealed class EnsureIdentityAdminStep : IStep<EnsureIdentityAdminOutput>, IValidatingStep
```

## Input Properties

```csharp
public string Username { get; set; } = "";
public string PasswordSecretName { get; set; } = "";
public string RoleName { get; set; } = "admin";
public string[] Permissions { get; set; } = [];
public bool AddMissingRolePermissions { get; set; }
```

## Output Shape

```csharp
public sealed record EnsureIdentityAdminOutput(
    string Username,
    string RoleName,
    bool UserCreated,
    bool UserAlreadyExisted,
    bool RoleCreated,
    bool RoleAlreadyExisted,
    bool RolePermissionsAdded,
    bool RoleAssigned);
```

The output must not include the password.

## Execution Target

The step targets live shell-scoped identity state using Elsa Identity services.

The target shell is not an input property. It is resolved from Loom execution context, using the Elsa convention key `elsa.shell`. Loom itself does not interpret this key; Elsa/CShells middleware should read it and establish the correct shell scope before validation and execution.

The step must not write `DefaultAdminUser` configuration in the initial implementation. Config-seeded admin users can remain a bootstrap compatibility path, but this recipe step reconciles actual identity state.

## Validation Rules

- The Loom execution context must contain `elsa.shell`.
- `Username` is required.
- `PasswordSecretName` is required when the user does not already exist.
- The referenced password secret must exist and resolve through secret management when the step needs to create the user.
- The step must not generate admin passwords or create password secrets. Human-facing credentials should be supplied deliberately through secret management before this step runs.
- `RoleName` is required.
- `Permissions` must not be empty.
- Permission entries must not be blank.
- The target shell must exist and have identity enabled.
- The identity persistence provider must be reachable enough to query users/roles.
- Validation may query shell-scoped identity services to determine whether the user already exists. If validation cannot determine user existence because identity services or persistence are unavailable, validation should fail.
- User and role lookup semantics should defer to Elsa Identity store/manager normalization rules. The step should not implement its own case-sensitivity policy.

## Idempotency

The step is idempotent:

- Execution order should be: ensure role, ensure user, ensure role assignment. If role assignment fails after user creation, a re-run should be able to repair the missing assignment.
- If the role exists, do not recreate it.
- If the role is missing, create it with `Permissions`.
- If the role exists and `AddMissingRolePermissions` is `false`, leave permissions unchanged. If existing permissions differ from `Permissions`, execution should succeed but emit a warning/diagnostic that role permissions drift from the recipe.
- If the role exists and `AddMissingRolePermissions` is `true`, add permissions that are present in `Permissions` but missing from the role. Do not remove extra permissions in v1.
- If the user exists, do not recreate it.
- If the user is missing, resolve `PasswordSecretName`, create the user, and assign `RoleName`.
- If the user exists, ensure the `RoleName` assignment if Elsa Identity exposes a safe role-assignment API.
- If the user already exists, do not resolve `PasswordSecretName` and do not modify the user's password.

When existing users or roles are found, outputs should report the actual stored user and role names where available.

## Required Services

Shell-scoped services:

- `IUserStore`
- `IRoleStore`
- `IUserManager`
- `IRoleManager`
- Secret management service capable of resolving `PasswordSecretName`.

Host services:

- A shell service scope resolver/factory for the context-selected shell.
- `ILogger<EnsureIdentityAdminStep>`.

Potential shared helper:

- `IElsaShellServiceAccessor` to execute work inside a named shell scope.

## Recipe Example

The referenced password secret can be backed by an environment variable for Docker quick-start scenarios:

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

```json
{
  "id": "default-shell",
  "context": {
    "elsa.shell": "Default"
  },
  "steps": [
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
  ]
}
```

The wildcard permission is intentionally explicit in the example. Recipes should make full-admin access visible instead of relying on an implicit default.

Inside a shell-scoped group:

```json
{
  "type": "elsa.identity.admin.ensure",
  "id": "admin",
  "input": {
    "username": "admin",
    "passwordSecretName": "[js: variables('adminPasswordSecretName')]",
    "roleName": "admin",
    "permissions": ["*"]
  }
}
```

## Relationship To Existing Code

The repo currently has `AdminUserInitializer`, which reads `ELSA_ADMIN_USER`, `ELSA_ADMIN_PASSWORD`, `ELSA_ADMIN_ROLE_NAME`, and `ELSA_ADMIN_ROLE_PERMISSIONS`.

This recipe step should supersede that environment-variable path for recipe-driven setup. The existing hosted service can remain for compatibility, but the recipe step should use the same Elsa Identity stores/managers directly so it can report idempotent outcomes. It should not write `DefaultAdminUser` configuration.

## Open Questions

- What is the exact shell-scoped service access API available from CShells in this host?
- Does Elsa Identity expose a role assignment API that is appropriate for idempotent reconciliation?
- What is the simplest companion setup story for creating/importing the password secret before this step runs?
- What final Loom JSON shape should represent grouped steps and inherited execution context?

## Deferred

- Bulk users and roles.
- Non-admin role templates.
- Password rotation policy.
- Password reset/update. This should be a separate explicit step, not part of admin ensure.
- Password secret creation/import. This likely belongs to a generic secret-management recipe step, not this identity admin step.
- Exact role permission replacement/removal.
- External identity providers.
- Invitation flows.

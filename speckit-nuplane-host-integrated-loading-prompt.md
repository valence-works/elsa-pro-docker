# Speckit Prompt: Nuplane Host-Integrated Assembly Loading

Create a new Speckit specification for adding first-class host-integrated assembly loading to Nuplane.

## Context

Nuplane currently loads resolved NuGet package assemblies into custom collectible `AssemblyLoadContext` instances. It also supports a shared assembly policy so plugin packages can use host-provided abstractions/contracts, e.g. `CShells.Abstractions` or `Microsoft.Extensions.*` abstractions. This works for type identity when the host scans plugin assemblies for implementations.

However, collectible loading breaks intended framework-integrated plugin scenarios. Example: an Elsa host uses Nuplane to load `Elsa.Persistence.EFCore.PostgreSql` dynamically. CShells can discover the feature from the Nuplane-loaded assembly, but later EF Core migrations call `Assembly.Load("Elsa.Persistence.EFCore.PostgreSql")` from EF/default app code. Because the assembly exists only in Nuplane’s collectible context, EF cannot resolve it. If the host bridges the default resolver to return the collectible assembly, .NET throws: “A non-collectible assembly may not reference a collectible assembly.”

This shows shared assemblies solve type identity, but not assembly lifetime/discoverability for framework code that later resolves or caches plugin assemblies.

## Goal

Specify a Nuplane-owned solution so host applications do not need custom `AssemblyLoadContext` or resolver plumbing for intended framework-integrated packages.

## Problem Statement

Nuplane needs to distinguish:

1. Shared assemblies/type identity: which assemblies should come from the host/default context so host/plugin contracts match.
2. Package load mode/lifetime: whether package assemblies are unloadable/collectible or stable for framework integration.
3. Assembly resolution visibility: whether framework code outside the plugin context can later resolve package assemblies by name.

## Primary Use Case

A host dynamically loads NuGet packages that contribute DI registrations, EF Core providers/migrations, ASP.NET endpoints, CShells/Elsa shell features, hosted services, options, validators, or similar framework-integrated types. These packages must be usable by framework code for the application lifetime without host-specific fixes.

## Desired Capability

Add a first-class host-integrated load mode to Nuplane.

## Potential Model

- Keep existing collectible load behavior for isolated/scan-only plugin scenarios.
- Add an explicit load mode, naming open to design but examples include:
  - `Collectible`
  - `NonCollectible`
  - `DefaultContext`
  - `HostIntegrated`
- Recommended conceptual split:
  - `SharedAssemblies` controls contract/type identity.
  - `PackageLoadMode` controls package assembly lifetime and load context behavior.
  - Nuplane-managed assembly resolution ensures framework `Assembly.Load` by name works for active host-integrated packages.

## Functional Requirements

- Allow configuring the default package load mode for autoloaded packages.
- Allow overriding load mode per package or per source/package request if the existing configuration model supports that cleanly.
- Preserve existing shared assembly policy semantics.
- For host-integrated packages, expose assemblies through `IPackageAssemblyCatalog`, or a new appropriate catalog, such that callers receive assemblies safe for framework integration.
- Host-integrated assemblies must not be collectible assemblies returned to non-collectible host/framework code.
- Framework code calling `Assembly.Load` by assembly name should be able to resolve active host-integrated package assemblies when appropriate.
- Nuplane should keep package graph dependency resolution intact: package dependencies should resolve consistently with the loaded package graph.
- Version conflicts should be handled deterministically and surfaced with clear diagnostics.
- Do not require host applications to implement custom `AssemblyLoadContext` or `AssemblyLoadContext.Default.Resolving` handlers.
- Existing collectible behavior should remain available and should not be silently broken.
- Add clear observability/logging around chosen load mode, resolved assembly paths, conflicts, and failed resolution.
- Add documentation explaining when to use collectible vs host-integrated modes, and how shared assemblies relate but do not replace load mode.

## Important Design Considerations

- Host-integrated packages may not be unloadable in the same way as collectible packages. This tradeoff should be explicit.
- Direct `DefaultContext` loading provides maximum framework compatibility but lowest isolation.
- Non-collectible package-specific load contexts may preserve more isolation, but framework `Assembly.Load` by name behavior must still be solved.
- The spec should evaluate whether a new catalog is better than changing `IPackageAssemblyCatalog` semantics.
- Avoid over-reliance on auto-detection. Auto-detection of EF/MVC/endpoint patterns may be useful for diagnostics, but explicit configuration should be primary.
- Backward compatibility matters: existing consumers expecting collectible assemblies should not be surprised.

## Candidate API and Configuration Examples

Default loading mode:

```json
{
  "Nuplane": {
    "Loading": {
      "Enabled": true,
      "DefaultLoadMode": "HostIntegrated",
      "SharedAssemblies": [
        { "Name": "CShells.Abstractions" },
        { "Name": "Microsoft.Extensions.DependencyInjection.Abstractions" }
      ]
    }
  }
}
```

Per-package loading mode:

```json
{
  "Id": "Elsa.Persistence.EFCore.PostgreSql",
  "Version": "[3.8.0-preview,)",
  "LoadMode": "HostIntegrated"
}
```

Builder API:

```csharp
services.AddNuplane(configuration, nuplane =>
{
    nuplane.AutoloadPackages(configuration.GetSection("Loading"));
    nuplane.UseHostIntegratedAssemblyCatalog();
});
```

## Acceptance Criteria

- A package containing an EF Core migrations assembly can be loaded by Nuplane in host-integrated mode and EF Core can resolve its migrations assembly by name without host custom code.
- A CShells/Elsa-style host can discover `IShellFeature` implementations from host-integrated Nuplane packages and activate shell features successfully.
- Collectible mode remains supported and covered by tests.
- Shared assembly policy remains independent from load mode and has tests showing contract type identity still works.
- Host-integrated mode has tests proving returned assemblies are not collectible and do not trigger “non-collectible assembly may not reference a collectible assembly.”
- `Assembly.Load` by simple name/full name resolves expected host-integrated package assemblies or fails with clear diagnostics when ambiguous/conflicting.
- Configuration binding and builder APIs are documented and tested.
- Existing tests pass or are intentionally updated to reflect the new model.

## Output Instructions

Please produce the Speckit specification only: problem, goals, non-goals, user stories, functional requirements, edge cases, acceptance criteria, and open questions. Do not implement code.

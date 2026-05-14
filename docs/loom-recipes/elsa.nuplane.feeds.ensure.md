# Loom Recipe Step: elsa.nuplane.feeds.ensure

## Status

Initial locked draft for the first Elsa Pro Server Loom recipe slice.

## Purpose

Ensure Nuplane package feeds are configured so Elsa Pro Server can resolve optional runtime packages.

This step covers a common first-run need before enabling shell features such as PostgreSQL persistence, RabbitMQ messaging, or Quartz scheduling: the required package sources must be present in `Nuplane:Setup:Feeds`.

The step intentionally manages a feed set rather than a single feed. Feed order and replacement semantics matter, and the common Elsa Pro setup needs multiple feeds configured together.

## Step Type

`elsa.nuplane.feeds.ensure`

## Step Shape

Typed step:

```csharp
[Step("elsa.nuplane.feeds.ensure")]
public sealed class EnsureNuplaneFeedsStep : IStep<EnsureNuplaneFeedsOutput>, IValidatingStep
```

## Input Properties

```csharp
public NuplaneFeedInput[] Feeds { get; set; } = [];
public NuplaneFeedReconciliationMode Mode { get; set; } = NuplaneFeedReconciliationMode.Merge;
```

```csharp
public enum NuplaneFeedReconciliationMode
{
    Merge,
    Replace
}
```

```csharp
public sealed class NuplaneFeedInput
{
    public string Name { get; set; } = "";
    public string? ServiceIndex { get; set; }
    public string? DirectoryPath { get; set; }
    public string[] IncludePatterns { get; set; } = [];
    public NuplaneDirectoryFeedOptionsInput? Directory { get; set; }
}

public sealed class NuplaneDirectoryFeedOptionsInput
{
    public bool? Watch { get; set; }
    public TimeSpan? DebounceWindow { get; set; }
}
```

## Output Shape

```csharp
public sealed record EnsureNuplaneFeedsOutput(
    bool Changed,
    string[] Added,
    string[] Updated,
    string[] Unchanged,
    string[] Removed);
```

## Configuration Target

The step updates:

```json
{
  "Nuplane": {
    "Setup": {
      "Feeds": []
    }
  }
}
```

It does not install packages directly, configure global Nuplane setup options such as `AutomaticReconciliation` or `PollInterval`, or enable shell features. Package load mode belongs to a later step or to `elsa.shells.features.enable` when the feature implies a package.

## Validation Rules

- `Feeds` must contain at least one entry.
- Each feed `Name` is required and unique within the recipe input.
- A feed must specify exactly one source kind:
  - `ServiceIndex` for NuGet-compatible feeds.
  - `DirectoryPath` for local package directories.
- `ServiceIndex`, when present, must be an absolute HTTP/HTTPS URI.
- `DirectoryPath`, when present, must be a non-empty path.
- `DirectoryPath` values should be preserved as authored after basic non-empty validation. The step should not normalize relative/absolute paths because Nuplane resolves them in the host/container context.
- Feed credentials are not supported in v1.
- `IncludePatterns` may be empty, but entries must not be blank.
- Directory feed `DebounceWindow`, when present, must be non-negative.
- V1 should not validate network reachability. Feed availability belongs to a future verification step or runner mode.

## Idempotency

The step is idempotent:

- Feeds are matched by `Name`.
- Missing feeds are appended.
- Existing feeds with identical values are left unchanged.
- Existing feeds with the same name but different values are updated to the desired shape.
- If an existing feed has the same `Name` but a different source kind, for example `ServiceIndex` versus `DirectoryPath`, the step should update to the desired shape but emit a warning/diagnostic.
- Feed comparison includes ordered `IncludePatterns`. If the recipe provides patterns in a different order than existing configuration, the feed is considered different and is updated to recipe order.
- In `Merge` mode, unspecified existing feeds are preserved.
- In `Replace` mode, the final feed list is exactly `Feeds`.

Feed order should be stable:

- Preserve existing order for unchanged or updated feeds.
- Append newly added feeds in recipe order when `Mode` is `Merge`.
- Use recipe order when `Mode` is `Replace`.

## Required Services

Initial implementation services:

- `IConfiguration` or a Nuplane configuration document abstraction.
- A JSON configuration writer/merger for `/config/config.json` or another selected recipe target.
- `ILogger<EnsureNuplaneFeedsStep>`.

Potential shared helper:

- `INuplaneConfigurationStore` to read and patch `Nuplane:Setup`.

## Recipe Example

```json
{
  "type": "elsa.nuplane.feeds.ensure",
  "id": "nuplane-feeds",
  "input": {
    "feeds": [
      {
        "name": "nuget.org",
        "serviceIndex": "https://api.nuget.org/v3/index.json"
      },
      {
        "name": "feedz.io",
        "serviceIndex": "https://f.feedz.io/elsa-workflows/elsa-3/nuget/index.json",
        "includePatterns": [
          "Elsa.Persistence.EFCore.PostgreSql [3.8.0-preview,)",
          "Elsa.Scheduling.Quartz.EFCore.PostgreSql [3.8.0-preview,)",
          "Elsa.ServiceBus.MassTransit.RabbitMq [3.8.0-preview,)"
        ]
      },
      {
        "name": "local-packages",
        "directoryPath": "packages",
        "includePatterns": ["*"],
        "directory": {
          "watch": true,
          "debounceWindow": "00:00:01"
        }
      }
    ]
  }
}
```

## Relationship To Other Steps

Expected ordering:

1. `elsa.nuplane.feeds.ensure`
2. Later package/load-mode step, if we add one.
3. `elsa.shells.features.enable`

This step only ensures sources. It should not infer or mutate shell features from include patterns.

## Open Questions

- Should package load modes be handled here because feed include patterns are often paired with host-integrated packages, or should that be a separate `elsa.nuplane.loading.ensure` step?
- Should this step support credentials for private feeds in the first slice? Recommendation: defer; secrets need a clear storage story.
- Should local directory feeds require the path to exist at validation time? Recommendation: warn or skip for first slice because Docker mount timing can make host validation misleading.

## Deferred

- Private feed credentials backed by logical secret names from `elsa.secrets.ensure`.
- Package installation/reconciliation.
- Global Nuplane setup options such as automatic reconciliation and polling interval.
- Nuplane loading mode configuration.
- Network reachability checks.
- Package graph validation.

# Nuplane Investigation Notes

## Stale active graph after package loader/resolver fixes

While troubleshooting optional Elsa Quartz SQLite packages loaded through Nuplane, the generated state file at:

`src/ElsaProServer/bin/Debug/net10.0/.nuplane/store-state.json`

contained an active graph for `Elsa.Scheduling.Quartz.EFCore.Sqlite` with only the root package listed:

```json
{
  "rootPackageIds": ["Elsa.Scheduling.Quartz.EFCore.Sqlite"],
  "nodePackageIds": ["Elsa.Scheduling.Quartz.EFCore.Sqlite"]
}
```

That was suspicious because the package nuspec declares dependencies, including:

- `Elsa.Scheduling.Quartz`
- `AppAny.Quartz.EntityFrameworkCore.Migrations.SQLite`
- `Elsa.Persistence.EFCore.Common`
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Quartz.Serialization.Json`

Moving the generated `store-state.json` aside and restarting caused Nuplane to rebuild state from scratch, which made the stale/malformed graph easier to separate from the current resolver conflict.

Potential Nuplane bug to investigate: after resolver/loader behavior changes, Nuplane may preserve an older active graph that no longer reflects package metadata, instead of invalidating/rebuilding it when the desired package root and version are unchanged but graph expansion behavior has changed.


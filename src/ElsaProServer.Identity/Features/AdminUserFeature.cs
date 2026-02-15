using CShells.Features;
using Elsa.Extensions;
using ElsaProServer.Identity.HostedServices;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace ElsaProServer.Identity.Features;

/// <summary>
/// Feature that initializes an admin user from configuration if provided.
/// </summary>
[ShellFeature(
    DisplayName = "Admin User Initialization",
    Description = "Initializes an admin user from ELSA_ADMIN_USER and ELSA_ADMIN_PASSWORD configuration if provided",
    DependsOn = ["Identity"])]
[UsedImplicitly]
public class AdminUserFeature : IShellFeature
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddBackgroundTask<AdminUserInitializer>();
    }
}

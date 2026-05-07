using CShells.Lifecycle;
using Microsoft.Extensions.Primitives;

namespace ElsaProCombined;

internal sealed class ConfigChangeShellReloader(
    IConfiguration configuration,
    IShellRegistry shellRegistry,
    ILogger<ConfigChangeShellReloader> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ChangeToken.OnChange(configuration.GetReloadToken, () => OnConfigChanged(stoppingToken));
        return Task.CompletedTask;
    }

    private async void OnConfigChanged(CancellationToken stoppingToken)
    {
        logger.LogInformation("Configuration change detected — reloading all shells");

        try
        {
            await shellRegistry.ReloadActiveAsync(cancellationToken: stoppingToken);
            logger.LogInformation("All shells reloaded successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reload shells after configuration change");
        }
    }
}

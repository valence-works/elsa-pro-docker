using CShells.Features;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Quartz.Logging;

namespace ElsaProCombined;

/// <summary>
/// Keeps Quartz logging bound to the currently active shell service provider.
/// </summary>
/// <remarks>
/// Quartz stores its log provider statically. When a shell is reloaded, the previous shell's
/// <see cref="ILoggerFactory"/> can be disposed while Quartz still references it. This feature
/// resets the Quartz log provider before Quartz creates its container configuration processor.
/// </remarks>
[ShellFeature(
    "QuartzLogging",
    DisplayName = "Quartz Logging",
    Description = "Ensures Quartz uses the active shell logger factory.")]
internal sealed class QuartzLoggingShellFeature : IShellFeature, IPostConfigureShellServices
{
    private const string ContainerConfigurationProcessorTypeName = "Quartz.ContainerConfigurationProcessor";

    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services)
    {
    }

    /// <inheritdoc />
    public void PostConfigureServices(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(x => x.ServiceType.FullName == ContainerConfigurationProcessorTypeName);

        if (descriptor == null)
            return;

        services.Remove(descriptor);
        services.Add(ServiceDescriptor.Singleton(descriptor.ServiceType, serviceProvider =>
        {
            LogContext.SetCurrentLogProvider(serviceProvider.GetRequiredService<ILoggerFactory>());
            return ActivatorUtilities.CreateInstance(serviceProvider, descriptor.ServiceType);
        }));
    }
}

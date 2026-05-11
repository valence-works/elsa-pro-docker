using Elsa.Studio.Authentication.ElsaIdentity.BlazorServer.Extensions;
using Elsa.Studio.Authentication.ElsaIdentity.HttpMessageHandlers;
using Elsa.Studio.Authentication.ElsaIdentity.UI.Extensions;
using Elsa.Studio.Contracts;
using Elsa.Studio.Core.BlazorServer.Extensions;
using Elsa.Studio.Dashboard.Extensions;
using Elsa.Studio.Extensions;
using Elsa.Studio.Localization.Time;
using Elsa.Studio.Localization.Time.Providers;
using Elsa.Studio.Models;
using Elsa.Studio.Shell.Extensions;
using Elsa.Studio.Workflows.Designer.Extensions;
using Elsa.Studio.Workflows.Extensions;
using ElsaProStudio.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ElsaProStudio.Server;

public static class ElsaProStudioServerExtensions
{
    public static IServiceCollection AddElsaProStudioBlazorServer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRazorPages(options => options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute()));
        services.AddServerSideBlazor();

        var backendApiConfig = new BackendApiConfig
        {
            ConfigureBackendOptions = options => configuration.GetSection("Backend").Bind(options),
            ConfigureHttpClientBuilder = options => options.AuthenticationHandler = typeof(ElsaIdentityAuthenticatingApiHttpMessageHandler),
        };

        services.AddRazorComponents().AddInteractiveServerComponents(options =>
        {
            options.RootComponents.RegisterCustomElsaStudioElements();
            options.RootComponents.MaxJSRootComponents = 10000;
        });
        services.AddCore();
        services.AddShell();
        services.AddRemoteBackend(backendApiConfig);
        services.AddElsaIdentity();
        services.AddElsaIdentityUI();
        services.AddDashboardModule();
        services.AddWorkflowsModule();
        services.AddScoped<ITimeZoneProvider, LocalTimeZoneProvider>();
        services.AddScoped<IFeature, CustomThemeFeature>();

        return services;
    }
}

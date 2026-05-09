using Elsa.Studio.Authentication.ElsaIdentity.BlazorWasm.Extensions;
using Elsa.Studio.Authentication.ElsaIdentity.HttpMessageHandlers;
using Elsa.Studio.Authentication.ElsaIdentity.UI.Extensions;
using Elsa.Studio.Contracts;
using Elsa.Studio.Core.BlazorWasm.Extensions;
using Elsa.Studio.Dashboard.Extensions;
using Elsa.Studio.Extensions;
using Elsa.Studio.Localization.Time;
using Elsa.Studio.Localization.Time.Providers;
using Elsa.Studio.Models;
using Elsa.Studio.Shell;
using Elsa.Studio.Shell.Extensions;
using Elsa.Studio.Workflows.Designer.Extensions;
using Elsa.Studio.Workflows.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ElsaProStudio.Shared;

public static class ElsaProStudioClientExtensions
{
    public static WebAssemblyHostBuilder AddElsaProStudioClient(this WebAssemblyHostBuilder builder, string appElementSelector = "#app")
    {
        builder.RootComponents.Add<App>(appElementSelector);
        builder.RootComponents.Add<HeadOutlet>("head::after");
        builder.RootComponents.RegisterCustomElsaStudioElements();

        var services = builder.Services;
        var configuration = builder.Configuration;
        var backendUrl = configuration["Backend:Url"] ?? "elsa/api";
        var absoluteBackendUrl = new Uri(new(builder.HostEnvironment.BaseAddress), backendUrl);

        var backendApiConfig = new BackendApiConfig
        {
            ConfigureBackendOptions = options => options.Url = absoluteBackendUrl,
            ConfigureHttpClientBuilder = options => options.AuthenticationHandler = typeof(ElsaIdentityAuthenticatingApiHttpMessageHandler),
        };

        services.AddCore();
        services.AddShell();
        services.AddRemoteBackend(backendApiConfig);
        services.AddElsaIdentity();
        services.AddElsaIdentityUI();
        services.AddDashboardModule();
        services.AddWorkflowsModule();
        services.AddScoped<ITimeZoneProvider, LocalTimeZoneProvider>();
        services.AddScoped<IFeature, CustomThemeFeature>();

        return builder;
    }
}

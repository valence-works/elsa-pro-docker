using CShells.AspNetCore.Configuration;
using CShells.AspNetCore.Extensions;
using CShells.AspNetCore.Resolution;
using CShells.DependencyInjection;
using CShells.FastEndpoints.Features;
using Elsa.Expressions.JavaScript.ShellFeatures;
using Elsa.Http.ShellFeatures;
using Elsa.Resilience.ShellFeatures;
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
using Elsa.ShellFeatures;
using Elsa.Shells.Api.ShellFeatures;
using Elsa.Workflows.Api.ShellFeatures;
using Elsa.Workflows.Management.ShellFeatures;
using Elsa.Workflows.Runtime.Distributed.ShellFeatures;
using Elsa.Workflows.Runtime.ShellFeatures;
using ElsaProCombined;
using ElsaProCombined.Client;
using CShells.Resolution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nuplane;
using Nuplane.Loading.Hosting.Builder;
using Nuplane.Sources.Directory.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.WebHost.UseStaticWebAssets();
builder.Configuration.AddJsonFile("/config/config.json", optional: true, reloadOnChange: true);

var services = builder.Services;
var configuration = builder.Configuration;
var nuplaneConfiguration = configuration.GetSection("Nuplane");
var studioHostingModel = configuration.GetValue("Studio:HostingModel", StudioHostingModels.WebAssembly);
var useBlazorServer = string.Equals(studioHostingModel, StudioHostingModels.BlazorServer, StringComparison.OrdinalIgnoreCase);

services.AddNuplane(nuplaneConfiguration, nuplane =>
{
    nuplane.AddDirectoryFeedsFromConfiguration(nuplaneConfiguration);
    nuplane.AutoloadPackages(nuplaneConfiguration.GetSection("Loading"));
});

services.AddSingleton<NuplaneAssemblyProvider>();
services.AddSingleton(new WebRoutingShellResolverOptions
{
    ExcludePaths =
    [
        "/_blazor",
        "/_framework",
        "/_content",
        "/app.css",
        "/appsettings.json",
        "/favicon.ico",
        "/ElsaProCombined.styles.css",
        "/ElsaProCombined.modules.json"
    ]
});

builder.AddShells(shells => shells
    .WithHostAssemblies()
    .WithAssemblyProvider<NuplaneAssemblyProvider>()
    .WithAuthenticationAndAuthorization()
    .WithConfigurationProvider(configuration)
    .ConfigureAllShells(shell =>
    {
        shell.WithFeatures(
            typeof(ElsaFeature),
            typeof(DistributedRuntimeFeature),
            typeof(WorkflowsApiFeature),
            typeof(ShellsApiFeature),
            typeof(WorkflowManagementFeature),
            typeof(WorkflowRuntimeFeature),
            typeof(ResilienceFeature),
            typeof(CachingWorkflowDefinitionsFeature),
            typeof(CachingWorkflowRuntimeFeature),
            typeof(JavaScriptFeature),
            typeof(HttpCacheFeature),
            typeof(FastEndpointsFeature),
            typeof(ElsaFastEndpointsFeature),
            typeof(QuartzLoggingShellFeature));
    })
);

services.Replace(ServiceDescriptor.Singleton<IShellResolver, InfrastructurePathShellResolver>());
services.AddHostedService<ConfigChangeShellReloader>();
services.AddAuthentication();
services.AddAuthorization();

if (useBlazorServer)
{
    services
        .AddRazorPages(options => options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute()))
        .ConfigureApplicationPartManager(manager =>
        {
            var controllerFeatureProviders = manager.FeatureProviders
                .Where(x => x.GetType().FullName == "Microsoft.AspNetCore.Mvc.Controllers.ControllerFeatureProvider")
                .ToList();

            foreach (var controllerFeatureProvider in controllerFeatureProviders)
                manager.FeatureProviders.Remove(controllerFeatureProvider);
        });
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
}

var allowedOrigins = configuration.GetSection("Elsa:Cors:AllowedOrigins").Get<string[]>() ?? [];

services.AddCors(cors => cors
    .AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Contains("*")) policy.AllowAnyOrigin();
        else policy.WithOrigins(allowedOrigins);

        policy.AllowAnyHeader().AllowAnyMethod();
    }));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseCors();
if (!useBlazorServer)
    app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapShells();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();

if (useBlazorServer)
{
    app.MapRazorPages();
    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");
}
else
{
    app.MapFallbackToFile("index.html");
}

app.Run();

internal static class StudioHostingModels
{
    public const string WebAssembly = "WebAssembly";
    public const string BlazorServer = "BlazorServer";
}

using CShells.AspNetCore.Configuration;
using CShells.AspNetCore.Extensions;
using CShells.AspNetCore.Resolution;
using CShells.DependencyInjection;
using CShells.FastEndpoints.Features;
using Elsa.Expressions.JavaScript.ShellFeatures;
using Elsa.Http.ShellFeatures;
using Elsa.Resilience.ShellFeatures;
using Elsa.ShellFeatures;
using Elsa.Shells.Api.ShellFeatures;
using Elsa.Workflows.Api.ShellFeatures;
using Elsa.Workflows.Management.ShellFeatures;
using Elsa.Workflows.Runtime.Distributed.ShellFeatures;
using Elsa.Workflows.Runtime.ShellFeatures;
using ElsaProCombined;
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
            typeof(ElsaFastEndpointsFeature));
    })
);

services.AddHostedService<ConfigChangeShellReloader>();
services.AddAuthentication();
services.AddAuthorization();

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
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapShells();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();
app.MapFallbackToFile("index.html");

app.Run();

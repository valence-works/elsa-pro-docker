using CShells.AspNetCore.Configuration;
using CShells.AspNetCore.Extensions;
using CShells.DependencyInjection;
using Elsa.Expressions.JavaScript.ShellFeatures;
using Elsa.Http.ShellFeatures;
using Elsa.Resilience.ShellFeatures;
using Elsa.ShellFeatures;
using Elsa.Shells.Api.ShellFeatures;
using Elsa.Workflows.Api.ShellFeatures;
using Elsa.Workflows.Management.ShellFeatures;
using Elsa.Workflows.Runtime.Distributed.ShellFeatures;
using Elsa.Workflows.Runtime.ShellFeatures;
using ElsaProServer;
using Nuplane;
using Nuplane.Loading.Hosting.Builder;
using Nuplane.Sources.Directory.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
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

builder.AddShells(shells => shells
    .WithHostAssemblies()
    .WithAssemblyProvider<NuplaneAssemblyProvider>()
    .WithAuthenticationAndAuthorization()
    .WithConfigurationProvider(builder.Configuration)
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
            typeof(ElsaFastEndpointsFeature));
    })
);

services.AddAuthentication();
services.AddAuthorization();
var allowedOrigins = builder.Configuration.GetSection("Elsa:Cors:AllowedOrigins").Get<string[]>() ?? [];

services.AddCors(cors => cors
    .AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Contains("*")) policy.AllowAnyOrigin();
        else policy.WithOrigins(allowedOrigins);

        policy.AllowAnyHeader().AllowAnyMethod();
    }));

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors();
app.MapShells();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();

app.Run();
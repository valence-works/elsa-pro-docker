using CShells.AspNetCore.Configuration;
using CShells.AspNetCore.Extensions;
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
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuplane;
using Nuplane.Loading.Hosting.Builder;
using Nuplane.Sources.Directory.Configuration;

namespace ElsaProServer.Hosting;

public static class ElsaProWorkflowEngineHostingExtensions
{
    public static WebApplicationBuilder AddElsaProWorkflowEngine(this WebApplicationBuilder builder)
    {
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

        return builder;
    }
}

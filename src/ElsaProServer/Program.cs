using CShells.AspNetCore.Configuration;
using CShells.AspNetCore.Extensions;
using CShells.DependencyInjection;
using CShells.FastEndpoints.Features;
using Elsa.Expressions.JavaScript.ShellFeatures;
using Elsa.Http.ShellFeatures;
using Elsa.Resilience.ShellFeatures;
using Elsa.Scheduling.Quartz.ShellFeatures;
using Elsa.ServiceBus.MassTransit.ShellFeatures;
using Elsa.ShellFeatures;
using Elsa.Workflows.Api.ShellFeatures;
using Elsa.Workflows.Management.ShellFeatures;
using Elsa.Workflows.Runtime.Distributed.ShellFeatures;
using Elsa.Workflows.Runtime.ShellFeatures;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.AddShells(shells =>
{
    shells.WithAuthenticationAndAuthorization();
    shells.WithConfigurationProvider(builder.Configuration);
    shells.AddShell("Default", shell =>
    {
        shell.WithFeatures(
            typeof(ElsaFeature), 
            typeof(DistributedRuntimeFeature),
            typeof(WorkflowsApiFeature), 
            typeof(ResilienceFeature),
            typeof(CachingWorkflowDefinitionsFeature),
            typeof(CachingWorkflowRuntimeFeature),
            typeof(JavaScriptFeature), 
            typeof(QuartzSchedulerFeature),
            typeof(MassTransitWorkflowManagementFeature),
            typeof(MassTransitWorkflowDispatcherFeature),
            typeof(HttpCacheFeature));

        shell.WithFeature<FastEndpointsFeature>(feature =>
        {
            feature.EndpointRoutePrefix = "api";
        });
        shell.FromConfiguration(builder.Configuration.GetSection("Elsa:Shell"));
    });
});

services.AddAuthentication();
services.AddAuthorization();
services.AddHealthChecks();

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
app.MapHealthChecks("/health");

app.Run();
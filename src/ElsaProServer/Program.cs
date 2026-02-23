using CShells.AspNetCore.Configuration;
using CShells.AspNetCore.Extensions;
using CShells.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.AddShells(shells =>
{
    shells.WithAuthenticationAndAuthorization();
    shells.WithConfigurationProvider(builder.Configuration);
    shells.AddShell("Default", shell =>
    {
        shell.WithFeatures(
            "Elsa", 
            "DistributedRuntime", 
            "WorkflowsApi", 
            "Resilience", 
            "JavaScript", 
            "QuartzScheduler",
            "MassTransitWorkflowManagement",
            "MassTransitWorkflowDispatcher",
            "HttpCache");
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
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
        shell.WithFeatures("Elsa", "WorkflowsApi", "Resilience");
        shell.FromConfiguration(builder.Configuration.GetSection("Elsa:Shell"));
    });
});

services.AddAuthentication();
services.AddAuthorization();
services.AddHealthChecks();

// Add CORS
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

// Map health check endpoint
app.MapHealthChecks("/health");

// Log admin credential environment variables
LogAdminCredentials(app.Logger);

app.Run();

// Helper method to log admin credential setup instructions
void LogAdminCredentials(ILogger logger)
{
    var adminEmail = Environment.GetEnvironmentVariable("ELSA_ADMIN_EMAIL");
    var adminPassword = Environment.GetEnvironmentVariable("ELSA_ADMIN_PASSWORD");

    if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
    {
        logger.LogWarning("ELSA_ADMIN_EMAIL and ELSA_ADMIN_PASSWORD environment variables not set.");
        logger.LogWarning("To create an admin user, use the Elsa Identity API after startup.");
        logger.LogInformation("Server started successfully. Use the Elsa endpoints to manage workflows.");
    }
    else
    {
        logger.LogInformation("Admin credentials provided via environment variables.");
        logger.LogInformation("Email: {Email}", adminEmail);
        logger.LogInformation("Use these credentials to create an admin user via the Identity API.");
    }
}
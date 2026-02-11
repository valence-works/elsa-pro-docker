using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Elsa services
builder.Services.AddElsa(elsa =>
{
    // Configure management features with EF Core
    elsa.UseWorkflowManagement(management =>
    {
        management.UseEntityFrameworkCore(ef => ef.UseSqlite(builder.Configuration.GetConnectionString("Elsa") ?? "Data Source=elsa.db"));
    });

    // Configure runtime features with EF Core
    elsa.UseWorkflowRuntime(runtime =>
    {
        runtime.UseEntityFrameworkCore(ef => ef.UseSqlite(builder.Configuration.GetConnectionString("Elsa") ?? "Data Source=elsa.db"));
    });

    // Configure Identity
    elsa.UseIdentity(identity =>
    {
        identity.TokenOptions = options => options.SigningKey = builder.Configuration["Elsa:Identity:SigningKey"] ?? "super-secret-signing-key-change-in-production";
    });

    elsa.UseDefaultAuthentication();
    elsa.UseHttp();
    elsa.UseJavaScript();
    elsa.UseLiquid();
});

// Add health checks
builder.Services.AddHealthChecks();

// Add CORS
var allowedOrigins = builder.Configuration.GetSection("Elsa:Cors:AllowedOrigins").Get<string[]>() 
    ?? new[] { "*" };

builder.Services.AddCors(cors => cors
    .AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(allowedOrigins);
        }
        
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("*");
    }));

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseWorkflows();

// Map health check endpoint
app.MapHealthChecks("/health");

// Log admin credential environment variables
LogAdminCredentials(app.Services, app.Logger);

app.Run();

// Helper method to log admin credential setup instructions
void LogAdminCredentials(IServiceProvider services, ILogger logger)
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

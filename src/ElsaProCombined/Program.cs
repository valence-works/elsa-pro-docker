using CShells.AspNetCore.Configuration;
using CShells.AspNetCore.Extensions;
using CShells.AspNetCore.Resolution;
using ElsaProCombined;
using ElsaProServer.Hosting;
using ElsaProStudio.Server;
using ElsaProStudio.Shared;
using CShells.Resolution;
using Elsa.Studio.Workflows.Designer.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.WebHost.UseStaticWebAssets();
builder.Configuration.AddJsonFile("/config/config.json", optional: true, reloadOnChange: true);

var services = builder.Services;
var configuration = builder.Configuration;
var studioHostingModel = configuration.GetValue("Studio:HostingModel", StudioHostingModels.WebAssembly);
var useBlazorServer = string.Equals(studioHostingModel, StudioHostingModels.BlazorServer, StringComparison.OrdinalIgnoreCase);

builder.AddElsaProWorkflowEngine();

services.Configure<DesignerOptions>(options => options.UseReactFlow = true);

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

services.Replace(ServiceDescriptor.Singleton<IShellResolver, InfrastructurePathShellResolver>());

if (useBlazorServer)
{
    services
        .AddElsaProStudioBlazorServer(configuration)
        .AddMvc()
        .ConfigureApplicationPartManager(manager =>
        {
            var controllerFeatureProviders = manager.FeatureProviders
                .Where(x => x.GetType().FullName == "Microsoft.AspNetCore.Mvc.Controllers.ControllerFeatureProvider")
                .ToList();

            foreach (var controllerFeatureProvider in controllerFeatureProviders)
                manager.FeatureProviders.Remove(controllerFeatureProvider);
        });
}

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

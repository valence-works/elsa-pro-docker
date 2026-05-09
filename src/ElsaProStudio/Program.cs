using ElsaProStudio.Server;
using ElsaProStudio.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.WebHost.UseStaticWebAssets();
builder.Configuration.AddJsonFile("/config/config.json", optional: true, reloadOnChange: true);

var services = builder.Services;
var configuration = builder.Configuration;
var studioHostingModel = configuration.GetValue("Studio:HostingModel", StudioHostingModels.WebAssembly);
var useBlazorServer = string.Equals(studioHostingModel, StudioHostingModels.BlazorServer, StringComparison.OrdinalIgnoreCase);

services.AddAuthentication();
services.AddAuthorization();

if (useBlazorServer)
{
    services.AddElsaProStudioBlazorServer(configuration);
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (!useBlazorServer)
    app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
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
    app.MapGet("/appsettings.json", () => new
    {
        Backend = new
        {
            Url = configuration.GetValue<string>("Backend:Url")
        }
    });
    app.MapFallbackToFile("index.html");
}

app.Run();

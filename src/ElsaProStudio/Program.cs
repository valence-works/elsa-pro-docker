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
    app.MapGet("/appsettings.json", () => GetClientConfiguration(configuration));
    app.MapFallbackToFile("index.html");
}

app.Run();

static IDictionary<string, object?> GetClientConfiguration(IConfiguration configuration)
{
    return GetSectionValue(configuration.GetSection("Studio:Client")) as IDictionary<string, object?> ?? new Dictionary<string, object?>();
}

static object? GetSectionValue(IConfigurationSection section)
{
    var children = section.GetChildren().ToArray();

    if (children.Length == 0)
        return section.Value;

    if (TryGetArrayIndexes(children, out var indexes))
        return children
            .Zip(indexes)
            .OrderBy(x => x.Second)
            .Select(x => GetSectionValue(x.First))
            .ToArray();

    return children.ToDictionary(
        child => child.Key,
        GetSectionValue,
        StringComparer.OrdinalIgnoreCase);
}

static bool TryGetArrayIndexes(IConfigurationSection[] children, out int[] indexes)
{
    indexes = new int[children.Length];

    for (var i = 0; i < children.Length; i++)
    {
        if (!int.TryParse(children[i].Key, out indexes[i]))
            return false;
    }

    return indexes
        .Order()
        .SequenceEqual(Enumerable.Range(0, children.Length));
}

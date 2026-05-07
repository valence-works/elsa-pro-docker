using Elsa.Studio.Authentication.ElsaIdentity.BlazorWasm.Extensions;
using Elsa.Studio.Authentication.ElsaIdentity.HttpMessageHandlers;
using Elsa.Studio.Authentication.ElsaIdentity.UI.Extensions;
using Elsa.Studio.Contracts;
using Elsa.Studio.Core.BlazorWasm.Extensions;
using Elsa.Studio.Dashboard.Extensions;
using Elsa.Studio.Extensions;
using Elsa.Studio.Localization.Time;
using Elsa.Studio.Localization.Time.Providers;
using Elsa.Studio.Models;
using Elsa.Studio.Shell;
using Elsa.Studio.Shell.Extensions;
using Elsa.Studio.Workflows.Designer.Extensions;
using Elsa.Studio.Workflows.Extensions;
using ElsaProCombined.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var services = builder.Services;
var configuration = builder.Configuration;
var backendUrl = configuration.GetValue<string>("Backend:Url") ?? "elsa/api";
var absoluteBackendUrl = new Uri(new Uri(builder.HostEnvironment.BaseAddress), backendUrl);

var backendApiConfig = new BackendApiConfig
{
    ConfigureBackendOptions = options => options.Url = absoluteBackendUrl,
    ConfigureHttpClientBuilder = options => options.AuthenticationHandler = typeof(ElsaIdentityAuthenticatingApiHttpMessageHandler),
};

services.AddCore();
services.AddShell();
services.AddRemoteBackend(backendApiConfig);
services.AddElsaIdentity();
services.AddElsaIdentityUI();
services.AddDashboardModule();
services.AddWorkflowsModule();
services.AddScoped<ITimeZoneProvider, LocalTimeZoneProvider>();
services.AddScoped<IFeature, CustomThemeFeature>();

await builder.Build().RunAsync();

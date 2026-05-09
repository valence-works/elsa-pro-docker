using ElsaProStudio.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.AddElsaProStudioClient();

await builder.Build().RunAsync();

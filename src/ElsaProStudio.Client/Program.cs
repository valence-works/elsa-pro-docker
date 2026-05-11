using ElsaProStudio.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

await builder.AddElsaProStudioClientAsync();

await builder.Build().RunAsync();

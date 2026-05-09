using CShells.AspNetCore.Extensions;
using ElsaProServer.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Configuration.AddJsonFile("/config/config.json", optional: true, reloadOnChange: true);

builder.AddElsaProWorkflowEngine();

var app = builder.Build();

app.UseCors();
app.UseRouting();
app.MapShells();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();

app.Run();

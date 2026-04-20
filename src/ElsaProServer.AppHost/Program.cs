var builder = DistributedApplication.CreateBuilder(args);

var server = builder.AddProject<Projects.ElsaProServer>("elsa-server");

builder.AddProject<Projects.ElsaProStudio_BlazorServer>("elsa-studio")
    .WithReference(server)
    .WaitFor(server);

builder.Build().Run();

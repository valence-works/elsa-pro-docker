var builder = DistributedApplication.CreateBuilder(args);

var server = builder.AddProject<Projects.ElsaProServer>("elsa-server");

builder.AddProject<Projects.ElsaProStudio>("elsa-studio")
    .WithReference(server)
    .WaitFor(server);

builder.Build().Run();

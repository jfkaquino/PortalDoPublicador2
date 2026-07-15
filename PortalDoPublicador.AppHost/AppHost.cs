using Google.Protobuf.WellKnownTypes;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.PortalDoPublicador_Api>("portaldopublicador-api");
builder.AddProject<Projects.PortalDoPublicador_Client>("portaldopublicador-client")
       .WithReference(api);

builder.Build().Run();

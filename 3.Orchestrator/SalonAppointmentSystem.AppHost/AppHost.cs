var builder = DistributedApplication.CreateBuilder(args);

// Configurar MySQL
var mysql = builder.AddMySql("mysql")
    .WithLifetime(ContainerLifetime.Persistent);

var salondb = mysql.AddDatabase("salondb");

var apiService = builder.AddProject<Projects.SalonAppointmentSystem_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(salondb)
    .WaitFor(salondb);

builder.AddProject<Projects.SalonAppointmentSystem_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

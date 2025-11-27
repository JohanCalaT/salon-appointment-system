var builder = DistributedApplication.CreateBuilder(args);

// Configurar SQL Server con volumen persistente para datos
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("salonappointment-sqlserver-data");  // Volumen para persistir datos

var salondb = sqlServer.AddDatabase("salondb");

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

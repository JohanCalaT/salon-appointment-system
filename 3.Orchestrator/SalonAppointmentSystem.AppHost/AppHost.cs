var builder = DistributedApplication.CreateBuilder(args);

// Configurar SQL Server con volumen persistente para datos
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("salonappointment-sqlserver-data");

var salondb = sqlServer.AddDatabase("salondb");

// Configurar Redis para cache y locks distribuidos
var redis = builder.AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("salonappointment-redis-data")
    .WithRedisInsight();  // UI para visualizar datos en desarrollo

var apiService = builder.AddProject<Projects.SalonAppointmentSystem_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(salondb)
    .WithReference(redis)
    .WaitFor(salondb)
    .WaitFor(redis);

builder.AddProject<Projects.SalonAppointmentSystem_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

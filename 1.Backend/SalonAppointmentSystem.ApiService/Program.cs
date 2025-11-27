using SalonAppointmentSystem.ApiService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add Infrastructure layer (DbContext with Pomelo MySQL, Identity, Repositories, etc.)
// Nota: NO usamos AddMySqlDataSource porque Pomelo maneja su propia conexión
// El connection string "salondb" es inyectado automáticamente por Aspire
builder.Services.AddInfrastructure(builder.Configuration);

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add controllers (for future API endpoints)
builder.Services.AddControllers();

var app = builder.Build();

// Initialize database (migrations + seeding)
if (app.Environment.IsDevelopment())
{
    await app.Services.InitializeDatabaseAsync();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Health check endpoint
app.MapGet("/", () => new
{
    Status = "Running",
    Service = "SalonAppointmentSystem API",
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName
});

// API info endpoint
app.MapGet("/api/info", () => new
{
    Message = "Salon Appointment System API",
    Documentation = "/openapi/v1.json",
    Health = "/health"
});

app.MapDefaultEndpoints();

app.Run();

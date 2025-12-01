using SalonAppointmentSystem.ApiService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add Infrastructure layer (DbContext, Identity, JWT, Repositories, etc.)
// El connection string "salondb" es inyectado automáticamente por Aspire
builder.Services.AddInfrastructure(builder.Configuration);

// Add services to the container.
builder.Services.AddProblemDetails();

// Add controllers
builder.Services.AddControllers();

// Configure Swagger/OpenAPI with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Salon Appointment System API",
        Version = "v1",
        Description = "API para el sistema de reservas de barbería"
    });

    // Configurar JWT en Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese el token JWT en el formato: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Initialize database (migrations + seeding)
if (app.Environment.IsDevelopment())
{
    await app.Services.InitializeDatabaseAsync();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Enable Swagger (siempre habilitado para facilitar pruebas)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Salon API v1");
    options.RoutePrefix = string.Empty; // Swagger en la raíz "/"
});

// Use authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// API info endpoint
app.MapGet("/api/info", () => new
{
    Message = "Salon Appointment System API",
    Version = "1.0.0",
    Documentation = "/swagger",
    Health = "/health",
    Environment = app.Environment.EnvironmentName
});

app.MapDefaultEndpoints();

app.Run();

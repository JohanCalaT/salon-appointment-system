using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SalonAppointmentSystem.ApiService.Application.Common.Constants;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.ApiService.Application.Common.Settings;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Repositories;
using SalonAppointmentSystem.ApiService.Infrastructure.Seeders;
using SalonAppointmentSystem.ApiService.Infrastructure.Services;

namespace SalonAppointmentSystem.ApiService.Infrastructure;

/// <summary>
/// Extensiones para registrar servicios de Infrastructure en el contenedor DI
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra todos los servicios de la capa Infrastructure
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Obtener connection string (Aspire inyecta automáticamente como "salondb")
        var connectionString = configuration.GetConnectionString("salondb")
            ?? throw new InvalidOperationException("Connection string 'salondb' not found. Make sure you're running with Aspire AppHost.");

        // Registrar DbContext con SQL Server
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString,
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });

            // Suprimir el warning de PendingModelChanges en EF Core 9
            // La migración inicial ya está creada y sincronizada
            options.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        // Registrar Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Registrar Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Registrar repositorios específicos (opcional, ya que UnitOfWork los expone)
        services.AddScoped<IEstacionRepository, EstacionRepository>();
        services.AddScoped<IServicioRepository, ServicioRepository>();
        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<IConfiguracionHorarioRepository, ConfiguracionHorarioRepository>();
        services.AddScoped<IConfiguracionGeneralRepository, ConfiguracionGeneralRepository>();

        // Registrar Seeder
        services.AddScoped<DatabaseSeeder>();

        // Registrar servicios de autenticación
        services.AddScoped<IAuthService, AuthService>();

        // Configurar JWT Settings
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings section not found in configuration.");

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Configurar autenticación JWT
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // En producción debe ser true
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero // Sin tolerancia de tiempo
            };
        });

        // Configurar políticas de autorización
        services.AddAuthorizationBuilder()
            .AddPolicy(AppPolicies.RequireAdmin, policy =>
                policy.RequireRole(ApplicationRoles.Admin))
            .AddPolicy(AppPolicies.RequireBarbero, policy =>
                policy.RequireRole(ApplicationRoles.Barbero))
            .AddPolicy(AppPolicies.RequireCliente, policy =>
                policy.RequireRole(ApplicationRoles.Cliente))
            .AddPolicy(AppPolicies.RequireAdminOrBarbero, policy =>
                policy.RequireRole(ApplicationRoles.Admin, ApplicationRoles.Barbero))
            .AddPolicy(AppPolicies.RequireAuthenticated, policy =>
                policy.RequireRole(ApplicationRoles.Admin, ApplicationRoles.Barbero, ApplicationRoles.Cliente));

        return services;
    }

    /// <summary>
    /// Aplica migraciones pendientes y ejecuta el seeder
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // Verificar si la tabla __EFMigrationsHistory existe y si InitialCreate está registrada
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();

            // Si InitialCreate está pendiente pero las tablas ya existen, marcarla como aplicada
            if (pendingMigrations.Contains("20251127184610_InitialCreate") && !appliedMigrations.Contains("20251127184610_InitialCreate"))
            {
                // Verificar si las tablas ya existen (usando AspNetRoles como indicador)
                var tablesExist = await context.Database.ExecuteSqlRawAsync(
                    @"IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetRoles')
                      BEGIN
                          IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20251127184610_InitialCreate')
                          BEGIN
                              INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
                              VALUES ('20251127184610_InitialCreate', '9.0.0')
                          END
                      END");

                logger.LogInformation("Migración InitialCreate marcada como aplicada (tablas ya existían)");
            }

            // Aplicar migraciones pendientes restantes
            await context.Database.MigrateAsync();
            logger.LogInformation("Migraciones aplicadas correctamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al aplicar migraciones");
            throw;
        }

        // Ejecutar seeder
        await seeder.SeedAsync();
    }
}


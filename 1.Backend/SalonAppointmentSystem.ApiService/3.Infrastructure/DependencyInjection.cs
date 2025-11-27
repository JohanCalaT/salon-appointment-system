using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Repositories;
using SalonAppointmentSystem.ApiService.Infrastructure.Seeders;

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
            options.UseSqlServer(connectionString,
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

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

        // Aplicar migraciones pendientes
        await context.Database.MigrateAsync();

        // Ejecutar seeder
        await seeder.SeedAsync();
    }
}


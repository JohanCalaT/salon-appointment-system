using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Seeders;

/// <summary>
/// Seeder principal para inicializar datos base en la aplicación
/// </summary>
public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta todos los seeders en orden
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            await SeedRolesAsync();
            await SeedAdminUserAsync();
            await SeedConfiguracionGeneralAsync();
            await SeedConfiguracionHorarioAsync();
            await SeedServiciosAsync();
            await SeedEstacionesAsync();

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database seeding");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in ApplicationRoles.TodosLosRoles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
                _logger.LogInformation("Created role: {RoleName}", roleName);
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@barberia.com";
        
        if (await _userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                NombreCompleto = "Administrador",
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, ApplicationRoles.Admin);
                _logger.LogInformation("Created admin user: {Email}", adminEmail);
            }
        }
    }

    private async Task SeedConfiguracionGeneralAsync()
    {
        var configuraciones = new List<ConfiguracionGeneral>
        {
            new() { Clave = ConfiguracionClaves.TiempoMinimoAnticipacionMinutos, Valor = "30", TipoDato = TipoDatoConfig.Entero, Descripcion = "Minutos mínimos de anticipación para reservar" },
            new() { Clave = ConfiguracionClaves.DiasMaximoReservaFutura, Valor = "30", TipoDato = TipoDatoConfig.Entero, Descripcion = "Días máximos para reservar en el futuro" },
            new() { Clave = ConfiguracionClaves.PermitirReservasInvitados, Valor = "true", TipoDato = TipoDatoConfig.Boolean, Descripcion = "Permitir reservas sin registro" },
            new() { Clave = ConfiguracionClaves.PuntosRequeridosDescuento, Valor = "100", TipoDato = TipoDatoConfig.Entero, Descripcion = "Puntos necesarios para un descuento" }
        };

        foreach (var config in configuraciones)
        {
            if (!await _context.ConfiguracionGeneral.AnyAsync(c => c.Clave == config.Clave))
            {
                await _context.ConfiguracionGeneral.AddAsync(config);
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedConfiguracionHorarioAsync()
    {
        if (await _context.ConfiguracionHorarios.AnyAsync()) return;

        var horarios = new List<ConfiguracionHorario>();
        
        // Lunes a Viernes: 9:00 - 20:00
        for (int i = 1; i <= 5; i++)
        {
            horarios.Add(new ConfiguracionHorario
            {
                DiaSemana = (DayOfWeek)i,
                HoraInicio = new TimeSpan(9, 0, 0),
                HoraFin = new TimeSpan(20, 0, 0),
                Activo = true
            });
        }

        // Sábado: 9:00 - 14:00
        horarios.Add(new ConfiguracionHorario
        {
            DiaSemana = DayOfWeek.Saturday,
            HoraInicio = new TimeSpan(9, 0, 0),
            HoraFin = new TimeSpan(14, 0, 0),
            Activo = true
        });

        await _context.ConfiguracionHorarios.AddRangeAsync(horarios);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created {Count} schedule configurations", horarios.Count);
    }

    private async Task SeedServiciosAsync()
    {
        if (await _context.Servicios.AnyAsync()) return;

        var servicios = new List<Servicio>
        {
            new() { Nombre = "Corte de cabello", Descripcion = "Corte clásico de cabello", DuracionMinutos = 30, Precio = 15.00m, PuntosQueOtorga = 15, Orden = 1 },
            new() { Nombre = "Corte + Barba", Descripcion = "Corte de cabello y arreglo de barba", DuracionMinutos = 45, Precio = 25.00m, PuntosQueOtorga = 25, Orden = 2 },
            new() { Nombre = "Afeitado clásico", Descripcion = "Afeitado con navaja y toalla caliente", DuracionMinutos = 30, Precio = 12.00m, PuntosQueOtorga = 12, Orden = 3 },
            new() { Nombre = "Arreglo de barba", Descripcion = "Perfilado y recorte de barba", DuracionMinutos = 20, Precio = 10.00m, PuntosQueOtorga = 10, Orden = 4 }
        };

        await _context.Servicios.AddRangeAsync(servicios);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created {Count} services", servicios.Count);
    }

    private async Task SeedEstacionesAsync()
    {
        if (await _context.Estaciones.AnyAsync()) return;

        var estaciones = new List<Estacion>
        {
            new() { Nombre = "Estación 1", Activa = true, Orden = 1 },
            new() { Nombre = "Estación 2", Activa = true, Orden = 2 },
            new() { Nombre = "Estación 3", Activa = true, Orden = 3 }
        };

        await _context.Estaciones.AddRangeAsync(estaciones);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created {Count} stations", estaciones.Count);
    }
}


using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using System.Reflection;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Persistence;

/// <summary>
/// DbContext principal de la aplicación con soporte para Identity
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets para las entidades del dominio
    public DbSet<Estacion> Estaciones => Set<Estacion>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<ConfiguracionHorario> ConfiguracionHorarios => Set<ConfiguracionHorario>();
    public DbSet<ConfiguracionGeneral> ConfiguracionGeneral => Set<ConfiguracionGeneral>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Llamar primero a la configuración base de Identity
        base.OnModelCreating(builder);

        // Aplicar todas las configuraciones de entidades desde el ensamblado actual
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configurar la relación ApplicationUser -> Estacion
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Estacion)
                  .WithOne()
                  .HasForeignKey<ApplicationUser>(u => u.EstacionId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Property(u => u.NombreCompleto)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.Property(u => u.FotoUrl)
                  .HasMaxLength(500);

            entity.HasIndex(u => u.Activo);
            entity.HasIndex(u => u.EstacionId);
        });
    }

    /// <summary>
    /// Override para auditoría automática de entidades
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Aquí se puede agregar lógica de auditoría automática si es necesario
        return await base.SaveChangesAsync(cancellationToken);
    }
}


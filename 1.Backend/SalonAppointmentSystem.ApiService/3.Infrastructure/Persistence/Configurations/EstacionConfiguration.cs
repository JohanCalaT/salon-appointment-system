using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalonAppointmentSystem.ApiService.Domain.Entities;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad Estacion
/// </summary>
public class EstacionConfiguration : IEntityTypeConfiguration<Estacion>
{
    public void Configure(EntityTypeBuilder<Estacion> builder)
    {
        builder.ToTable("Estaciones");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
               .ValueGeneratedOnAdd();

        builder.Property(e => e.Nombre)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(e => e.Descripcion)
               .HasMaxLength(500);

        builder.Property(e => e.BarberoId)
               .HasMaxLength(450); // Longitud estándar para Identity User Id

        builder.Property(e => e.Activa)
               .HasDefaultValue(true);

        builder.Property(e => e.Orden)
               .HasDefaultValue(0);

        builder.Property(e => e.UsaHorarioGenerico)
               .HasDefaultValue(true);

        // Índices
        builder.HasIndex(e => e.Nombre)
               .IsUnique();

        builder.HasIndex(e => e.BarberoId);

        builder.HasIndex(e => e.Activa);

        builder.HasIndex(e => new { e.Activa, e.BarberoId });

        // Relación con Reservas
        builder.HasMany(e => e.Reservas)
               .WithOne(r => r.Estacion)
               .HasForeignKey(r => r.EstacionId)
               .OnDelete(DeleteBehavior.Restrict);

        // Relación con Horarios se configura en ConfiguracionHorarioConfiguration

        // Ignorar propiedades calculadas
        builder.Ignore(e => e.TieneBarberoAsignado);
        builder.Ignore(e => e.PuedeRecibirReservas);
        builder.Ignore(e => e.DomainEvents);
    }
}


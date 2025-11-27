using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalonAppointmentSystem.ApiService.Domain.Entities;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad Servicio
/// </summary>
public class ServicioConfiguration : IEntityTypeConfiguration<Servicio>
{
    public void Configure(EntityTypeBuilder<Servicio> builder)
    {
        builder.ToTable("Servicios");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
               .ValueGeneratedOnAdd();

        builder.Property(s => s.Nombre)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(s => s.Descripcion)
               .HasMaxLength(500);

        builder.Property(s => s.DuracionMinutos)
               .IsRequired();

        builder.Property(s => s.Precio)
               .HasPrecision(10, 2)
               .IsRequired();

        builder.Property(s => s.PuntosQueOtorga)
               .HasDefaultValue(0);

        builder.Property(s => s.Activo)
               .HasDefaultValue(true);

        builder.Property(s => s.Orden)
               .HasDefaultValue(0);

        // Índices
        builder.HasIndex(s => s.Nombre)
               .IsUnique();

        builder.HasIndex(s => s.Activo);

        // Relación con Reservas
        builder.HasMany(s => s.Reservas)
               .WithOne(r => r.Servicio)
               .HasForeignKey(r => r.ServicioId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalonAppointmentSystem.ApiService.Domain.Entities;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad ConfiguracionGeneral
/// </summary>
public class ConfiguracionGeneralConfiguration : IEntityTypeConfiguration<ConfiguracionGeneral>
{
    public void Configure(EntityTypeBuilder<ConfiguracionGeneral> builder)
    {
        builder.ToTable("ConfiguracionGeneral");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
               .ValueGeneratedOnAdd();

        builder.Property(c => c.Clave)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(c => c.Valor)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(c => c.Descripcion)
               .HasMaxLength(500);

        builder.Property(c => c.TipoDato)
               .HasConversion<int>()
               .IsRequired();

        // Índice único para la clave
        builder.HasIndex(c => c.Clave)
               .IsUnique();

        // Ignorar propiedades de navegación de BaseEntity
        builder.Ignore(c => c.DomainEvents);
    }
}


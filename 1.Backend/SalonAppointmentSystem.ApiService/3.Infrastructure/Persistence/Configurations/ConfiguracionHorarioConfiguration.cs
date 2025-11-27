using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalonAppointmentSystem.ApiService.Domain.Entities;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad ConfiguracionHorario
/// </summary>
public class ConfiguracionHorarioConfiguration : IEntityTypeConfiguration<ConfiguracionHorario>
{
    public void Configure(EntityTypeBuilder<ConfiguracionHorario> builder)
    {
        builder.ToTable("ConfiguracionHorarios");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
               .ValueGeneratedOnAdd();

        builder.Property(c => c.DiaSemana)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(c => c.HoraInicio)
               .IsRequired();

        builder.Property(c => c.HoraFin)
               .IsRequired();

        builder.Property(c => c.Activo)
               .HasDefaultValue(true);

        // Índices
        builder.HasIndex(c => c.DiaSemana);
        builder.HasIndex(c => c.Activo);
        builder.HasIndex(c => new { c.DiaSemana, c.Activo });

        // Ignorar propiedades de navegación de BaseEntity
        builder.Ignore(c => c.DomainEvents);
    }
}


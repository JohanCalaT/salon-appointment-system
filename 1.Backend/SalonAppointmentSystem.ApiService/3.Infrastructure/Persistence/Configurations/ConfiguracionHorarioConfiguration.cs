using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.Shared.Enums;

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

        // FK a Estacion (nullable - null significa horario global)
        builder.Property(c => c.EstacionId);

        // Tipo de horario
        builder.Property(c => c.Tipo)
               .HasConversion<int>()
               .HasDefaultValue(TipoHorario.Regular);

        builder.Property(c => c.DiaSemana)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(c => c.HoraInicio)
               .IsRequired();

        builder.Property(c => c.HoraFin)
               .IsRequired();

        builder.Property(c => c.Activo)
               .HasDefaultValue(true);

        builder.Property(c => c.Descripcion)
               .HasMaxLength(500);

        // Índices
        builder.HasIndex(c => c.DiaSemana);
        builder.HasIndex(c => c.Activo);
        builder.HasIndex(c => c.EstacionId);
        builder.HasIndex(c => c.Tipo);
        builder.HasIndex(c => new { c.DiaSemana, c.Activo });
        builder.HasIndex(c => new { c.EstacionId, c.DiaSemana, c.Activo });
        builder.HasIndex(c => new { c.EstacionId, c.Tipo, c.Activo });

        // Relación con Estacion
        builder.HasOne(c => c.Estacion)
               .WithMany(e => e.Horarios)
               .HasForeignKey(c => c.EstacionId)
               .OnDelete(DeleteBehavior.Cascade); // Si se elimina la estación, se eliminan sus horarios

        // Ignorar propiedades de navegación de BaseEntity
        builder.Ignore(c => c.DomainEvents);
    }
}


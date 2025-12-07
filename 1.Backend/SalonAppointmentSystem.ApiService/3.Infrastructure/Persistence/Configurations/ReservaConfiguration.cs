using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalonAppointmentSystem.ApiService.Domain.Entities;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad Reserva
/// </summary>
public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.ToTable("Reservas");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
               .ValueGeneratedOnAdd();

        // Código único para consulta de invitados
        builder.Property(r => r.CodigoReserva)
               .HasMaxLength(8)
               .IsRequired();

        builder.Property(r => r.UsuarioId)
               .HasMaxLength(450); // Nullable para invitados

        // Campos de auditoría - quién creó la reserva
        builder.Property(r => r.CreadaPor)
               .HasMaxLength(450);

        builder.Property(r => r.RolCreador)
               .HasMaxLength(50);

        builder.Property(r => r.NombreCliente)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(r => r.EmailCliente)
               .HasMaxLength(256)
               .IsRequired();

        builder.Property(r => r.TelefonoCliente)
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(r => r.FechaHora)
               .IsRequired();

        builder.Property(r => r.DuracionMinutos)
               .IsRequired();

        builder.Property(r => r.Estado)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(r => r.TipoReserva)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(r => r.PuntosGanados)
               .HasDefaultValue(0);

        builder.Property(r => r.Precio)
               .HasPrecision(10, 2)
               .IsRequired();

        builder.Property(r => r.FechaCreacion)
               .IsRequired();

        builder.Property(r => r.CanceladaPor)
               .HasMaxLength(450);

        builder.Property(r => r.MotivoCancelacion)
               .HasMaxLength(500);

        // Índices para búsquedas frecuentes
        builder.HasIndex(r => r.EstacionId);
        builder.HasIndex(r => r.UsuarioId);
        builder.HasIndex(r => r.ServicioId);
        builder.HasIndex(r => r.FechaHora);
        builder.HasIndex(r => r.Estado);
        builder.HasIndex(r => r.EmailCliente);

        // Índice único para código de reserva (búsqueda de invitados)
        builder.HasIndex(r => r.CodigoReserva)
               .IsUnique();

        // Índice compuesto para búsqueda de solapamientos (crítico para rendimiento)
        builder.HasIndex(r => new { r.EstacionId, r.FechaHora, r.Estado })
               .HasDatabaseName("IX_Reserva_EstacionId_FechaHora_Estado");

        // Ignorar propiedades calculadas
        builder.Ignore(r => r.FechaHoraFin);
        builder.Ignore(r => r.DomainEvents);
    }
}


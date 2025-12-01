using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalonAppointmentSystem.ApiService.Domain.Entities;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad RefreshToken
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
               .ValueGeneratedOnAdd();

        builder.Property(r => r.Token)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(r => r.UserId)
               .HasMaxLength(450)
               .IsRequired();

        builder.Property(r => r.ExpiresAt)
               .IsRequired();

        builder.Property(r => r.CreatedAt)
               .IsRequired();

        builder.Property(r => r.ReplacedByToken)
               .HasMaxLength(500);

        builder.Property(r => r.ReasonRevoked)
               .HasMaxLength(256);

        builder.Property(r => r.CreatedByIp)
               .HasMaxLength(50);

        builder.Property(r => r.RevokedByIp)
               .HasMaxLength(50);

        // Índices para búsquedas frecuentes
        builder.HasIndex(r => r.Token);
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => new { r.UserId, r.RevokedAt });

        // Ignorar propiedades calculadas
        builder.Ignore(r => r.IsActive);
        builder.Ignore(r => r.IsExpired);
        builder.Ignore(r => r.DomainEvents);
    }
}


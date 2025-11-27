namespace SalonAppointmentSystem.ApiService.Domain.Common;

/// <summary>
/// Clase base para entidades que requieren auditoría (fechas de creación/modificación)
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaModificacion { get; set; }

    public string? CreadoPor { get; set; }

    public string? ModificadoPor { get; set; }
}


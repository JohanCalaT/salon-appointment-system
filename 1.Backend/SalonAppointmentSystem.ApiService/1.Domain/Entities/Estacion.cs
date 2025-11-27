using SalonAppointmentSystem.ApiService.Domain.Common;

namespace SalonAppointmentSystem.ApiService.Domain.Entities;

/// <summary>
/// Representa una estación/silla de trabajo en la barbería
/// </summary>
public class Estacion : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// ID del barbero asignado a esta estación (FK a AspNetUsers)
    /// Nullable porque una estación puede estar sin barbero asignado
    /// </summary>
    public string? BarberoId { get; set; }

    /// <summary>
    /// Indica si la estación está activa y disponible para reservas
    /// </summary>
    public bool Activa { get; set; } = true;

    /// <summary>
    /// Orden de visualización en la UI
    /// </summary>
    public int Orden { get; set; }

    // Propiedades de navegación (virtual para lazy loading)
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}


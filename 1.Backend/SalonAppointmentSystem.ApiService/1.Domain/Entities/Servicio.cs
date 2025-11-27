using SalonAppointmentSystem.ApiService.Domain.Common;

namespace SalonAppointmentSystem.ApiService.Domain.Entities;

/// <summary>
/// Representa un servicio ofrecido por la barbería (corte, barba, etc.)
/// </summary>
public class Servicio : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    /// <summary>
    /// Duración del servicio en minutos
    /// </summary>
    public int DuracionMinutos { get; set; }

    /// <summary>
    /// Precio del servicio
    /// </summary>
    public decimal Precio { get; set; }

    /// <summary>
    /// Puntos de fidelidad que otorga este servicio al completarse
    /// </summary>
    public int PuntosQueOtorga { get; set; }

    /// <summary>
    /// Indica si el servicio está activo y disponible para reservas
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Orden de visualización en la UI
    /// </summary>
    public int Orden { get; set; }

    // Propiedades de navegación (virtual para lazy loading)
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}


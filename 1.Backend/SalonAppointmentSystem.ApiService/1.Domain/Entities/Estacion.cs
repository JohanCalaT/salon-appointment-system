using SalonAppointmentSystem.ApiService.Domain.Common;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;

namespace SalonAppointmentSystem.ApiService.Domain.Entities;

/// <summary>
/// Representa una estación/silla de trabajo en la barbería
/// </summary>
public class Estacion : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción o notas sobre la estación
    /// </summary>
    public string? Descripcion { get; set; }

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

    /// <summary>
    /// Si es true, la estación usa el horario global del negocio.
    /// Si es false, usa sus propios horarios personalizados.
    /// </summary>
    public bool UsaHorarioGenerico { get; set; } = true;

    // Propiedades de navegación (virtual para lazy loading)

    /// <summary>
    /// Barbero asignado a esta estación
    /// </summary>
    public virtual ApplicationUser? Barbero { get; set; }

    /// <summary>
    /// Reservas asociadas a esta estación
    /// </summary>
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    /// <summary>
    /// Horarios personalizados de esta estación
    /// </summary>
    public virtual ICollection<ConfiguracionHorario> Horarios { get; set; } = new List<ConfiguracionHorario>();

    // Métodos de dominio

    /// <summary>
    /// Indica si la estación tiene un barbero asignado
    /// </summary>
    public bool TieneBarberoAsignado => !string.IsNullOrEmpty(BarberoId);

    /// <summary>
    /// Indica si la estación puede recibir reservas
    /// (debe estar activa y tener barbero asignado)
    /// </summary>
    public bool PuedeRecibirReservas => Activa && TieneBarberoAsignado;
}


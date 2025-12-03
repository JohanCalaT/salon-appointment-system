namespace SalonAppointmentSystem.Shared.DTOs.Estaciones;

/// <summary>
/// DTO de respuesta para una estación
/// </summary>
public class EstacionDto
{
    /// <summary>
    /// Identificador único de la estación
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre de la estación (ej: "Estación 1", "Silla Principal")
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción o notas sobre la estación
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// ID del barbero asignado (null si no tiene barbero)
    /// </summary>
    public string? BarberoId { get; set; }

    /// <summary>
    /// Nombre completo del barbero asignado
    /// </summary>
    public string? NombreBarbero { get; set; }

    /// <summary>
    /// Email del barbero asignado
    /// </summary>
    public string? EmailBarbero { get; set; }

    /// <summary>
    /// Indica si la estación está activa
    /// </summary>
    public bool Activa { get; set; }

    /// <summary>
    /// Orden de visualización en la UI
    /// </summary>
    public int Orden { get; set; }

    /// <summary>
    /// Si es true, usa el horario global del negocio
    /// </summary>
    public bool UsaHorarioGenerico { get; set; }

    /// <summary>
    /// Indica si la estación puede recibir reservas (activa y con barbero)
    /// </summary>
    public bool PuedeRecibirReservas { get; set; }

    /// <summary>
    /// Cantidad de reservas pendientes/confirmadas para hoy
    /// </summary>
    public int ReservasHoy { get; set; }
}


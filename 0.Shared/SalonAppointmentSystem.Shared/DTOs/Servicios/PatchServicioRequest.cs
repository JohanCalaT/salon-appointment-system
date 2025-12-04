namespace SalonAppointmentSystem.Shared.DTOs.Servicios;

/// <summary>
/// Request para actualización parcial de un servicio (PATCH)
/// Solo se actualizan los campos que no son null
/// </summary>
public class PatchServicioRequest
{
    /// <summary>
    /// Nombre del servicio (si se proporciona, debe ser único)
    /// </summary>
    public string? Nombre { get; set; }

    /// <summary>
    /// Descripción detallada del servicio
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Duración del servicio en minutos
    /// </summary>
    public int? DuracionMinutos { get; set; }

    /// <summary>
    /// Precio del servicio
    /// </summary>
    public decimal? Precio { get; set; }

    /// <summary>
    /// Puntos de fidelidad que otorga
    /// </summary>
    public int? PuntosQueOtorga { get; set; }

    /// <summary>
    /// Indica si el servicio está activo
    /// </summary>
    public bool? Activo { get; set; }

    /// <summary>
    /// Orden de visualización
    /// </summary>
    public int? Orden { get; set; }
}


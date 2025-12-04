namespace SalonAppointmentSystem.Shared.DTOs.Servicios;

/// <summary>
/// Request para actualizar completamente un servicio (PUT)
/// </summary>
public class UpdateServicioRequest
{
    /// <summary>
    /// Nombre del servicio (requerido, único)
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del servicio
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Duración del servicio en minutos (requerido, mayor a 0)
    /// </summary>
    public int DuracionMinutos { get; set; }

    /// <summary>
    /// Precio del servicio (requerido, mayor a 0)
    /// </summary>
    public decimal Precio { get; set; }

    /// <summary>
    /// Puntos de fidelidad que otorga
    /// </summary>
    public int PuntosQueOtorga { get; set; }

    /// <summary>
    /// Indica si el servicio está activo
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Orden de visualización
    /// </summary>
    public int Orden { get; set; }
}


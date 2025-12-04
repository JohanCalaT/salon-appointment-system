namespace SalonAppointmentSystem.Shared.DTOs.Servicios;

/// <summary>
/// Request para crear un nuevo servicio
/// </summary>
public class CreateServicioRequest
{
    /// <summary>
    /// Nombre del servicio (requerido, único)
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del servicio (opcional)
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
    /// Puntos de fidelidad que otorga (opcional, default 0)
    /// </summary>
    public int PuntosQueOtorga { get; set; } = 0;

    /// <summary>
    /// Orden de visualización (opcional, default 0)
    /// </summary>
    public int Orden { get; set; } = 0;
}


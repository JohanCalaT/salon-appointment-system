namespace SalonAppointmentSystem.Shared.DTOs.Servicios;

/// <summary>
/// DTO de respuesta para un servicio de barbería
/// </summary>
public class ServicioDto
{
    /// <summary>
    /// Identificador único del servicio
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre del servicio (ej: "Corte clásico", "Barba completa")
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del servicio
    /// </summary>
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
    public bool Activo { get; set; }

    /// <summary>
    /// Orden de visualización en la UI
    /// </summary>
    public int Orden { get; set; }
}


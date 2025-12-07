using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.Shared.DTOs.Reservas;

/// <summary>
/// Request para crear una nueva reserva
/// </summary>
public record CreateReservaRequest
{
    /// <summary>
    /// ID de la estación donde se realizará el servicio
    /// </summary>
    public int EstacionId { get; init; }
    
    /// <summary>
    /// ID del servicio a realizar
    /// </summary>
    public int ServicioId { get; init; }
    
    /// <summary>
    /// Fecha y hora de la reserva (UTC)
    /// </summary>
    public DateTime FechaHora { get; init; }
    
    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string NombreCliente { get; init; } = string.Empty;
    
    /// <summary>
    /// Email del cliente para contacto
    /// </summary>
    public string EmailCliente { get; init; } = string.Empty;
    
    /// <summary>
    /// Teléfono del cliente para contacto
    /// </summary>
    public string TelefonoCliente { get; init; } = string.Empty;
    
    /// <summary>
    /// Tipo de reserva (Manual o CitaRapida)
    /// </summary>
    public TipoReserva TipoReserva { get; init; } = TipoReserva.Manual;
}

/// <summary>
/// Request para cancelar una reserva
/// </summary>
public record CancelReservaRequest
{
    /// <summary>
    /// Motivo de la cancelación
    /// </summary>
    public string? Motivo { get; init; }
}

/// <summary>
/// Request para actualizar una reserva (solo Admin/Barbero)
/// </summary>
public record UpdateReservaRequest
{
    /// <summary>
    /// Nueva fecha y hora (opcional)
    /// </summary>
    public DateTime? FechaHora { get; init; }
    
    /// <summary>
    /// Nuevo ID de estación (opcional)
    /// </summary>
    public int? EstacionId { get; init; }
    
    /// <summary>
    /// Nuevo nombre del cliente (opcional)
    /// </summary>
    public string? NombreCliente { get; init; }
    
    /// <summary>
    /// Nuevo email del cliente (opcional)
    /// </summary>
    public string? EmailCliente { get; init; }
    
    /// <summary>
    /// Nuevo teléfono del cliente (opcional)
    /// </summary>
    public string? TelefonoCliente { get; init; }
    
    /// <summary>
    /// Nuevo estado de la reserva (opcional)
    /// </summary>
    public EstadoReserva? Estado { get; init; }
}

/// <summary>
/// Request para buscar reserva por código (invitados)
/// </summary>
public record BuscarReservaPorCodigoRequest
{
    /// <summary>
    /// Código de reserva de 8 caracteres
    /// </summary>
    public string Codigo { get; init; } = string.Empty;

    /// <summary>
    /// Email del cliente para verificación
    /// </summary>
    public string Email { get; init; } = string.Empty;
}

/// <summary>
/// Request para cancelar reserva como invitado
/// </summary>
public record CancelReservaAnonimaRequest
{
    /// <summary>
    /// Código de reserva de 8 caracteres
    /// </summary>
    public string Codigo { get; init; } = string.Empty;

    /// <summary>
    /// Email del cliente para verificación
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Motivo de la cancelación (opcional)
    /// </summary>
    public string? Motivo { get; init; }
}


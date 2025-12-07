using MediatR;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.UpdateReserva;

/// <summary>
/// Command para actualizar una reserva existente (solo Admin/Barbero)
/// </summary>
public record UpdateReservaCommand : IRequest<Result<ReservaDto>>
{
    /// <summary>
    /// ID de la reserva a actualizar
    /// </summary>
    public int ReservaId { get; init; }
    
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
    
    /// <summary>
    /// ID del usuario que actualiza
    /// </summary>
    public string ActualizadaPor { get; init; } = string.Empty;
    
    /// <summary>
    /// Rol del usuario que actualiza
    /// </summary>
    public string RolActualizador { get; init; } = string.Empty;
    
    /// <summary>
    /// Crea un comando desde el request del controller
    /// </summary>
    public static UpdateReservaCommand FromRequest(
        int reservaId,
        UpdateReservaRequest request,
        string actualizadaPor,
        string rolActualizador)
    {
        return new UpdateReservaCommand
        {
            ReservaId = reservaId,
            FechaHora = request.FechaHora,
            EstacionId = request.EstacionId,
            NombreCliente = request.NombreCliente,
            EmailCliente = request.EmailCliente,
            TelefonoCliente = request.TelefonoCliente,
            Estado = request.Estado,
            ActualizadaPor = actualizadaPor,
            RolActualizador = rolActualizador
        };
    }
}


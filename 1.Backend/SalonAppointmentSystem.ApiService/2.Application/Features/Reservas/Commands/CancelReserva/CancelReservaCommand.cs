using MediatR;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.CancelReserva;

/// <summary>
/// Command para cancelar una reserva
/// </summary>
public record CancelReservaCommand : IRequest<Result<ReservaDto>>
{
    /// <summary>
    /// ID de la reserva a cancelar
    /// </summary>
    public int ReservaId { get; init; }
    
    /// <summary>
    /// Motivo de la cancelación
    /// </summary>
    public string? MotivoCancelacion { get; init; }
    
    /// <summary>
    /// ID del usuario que cancela (o "Invitado")
    /// </summary>
    public string CanceladaPor { get; init; } = string.Empty;
    
    /// <summary>
    /// Rol del usuario que cancela
    /// </summary>
    public string? RolCancelador { get; init; }
    
    /// <summary>
    /// Indica si es una cancelación por código (invitado)
    /// </summary>
    public bool EsCancelacionPorCodigo { get; init; }
    
    /// <summary>
    /// Email para verificación (solo para cancelación por código)
    /// </summary>
    public string? EmailVerificacion { get; init; }
}


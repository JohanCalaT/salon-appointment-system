using MediatR;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.Shared.DTOs.Reservas;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.CompletarReserva;

/// <summary>
/// Command para marcar una reserva como completada (servicio realizado)
/// Solo puede ser ejecutado por Admin o Barbero
/// </summary>
public record CompletarReservaCommand : IRequest<Result<ReservaDto>>
{
    /// <summary>
    /// ID de la reserva a completar
    /// </summary>
    public int ReservaId { get; init; }
    
    /// <summary>
    /// ID del usuario que completa la reserva
    /// </summary>
    public string CompletadaPor { get; init; } = string.Empty;
}


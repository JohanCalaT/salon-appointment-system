using MediatR;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.Shared.DTOs.Reservas;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetReservaByCodigo;

/// <summary>
/// Query para que invitados consulten su reserva por código
/// Requiere verificación de email para seguridad
/// </summary>
public record GetReservaByCodigoQuery : IRequest<Result<ReservaDto>>
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


using MediatR;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.Shared.DTOs.Reservas;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetReservaById;

/// <summary>
/// Query para obtener una reserva por su ID
/// Incluye validación de permisos según el rol del usuario
/// </summary>
public record GetReservaByIdQuery : IRequest<Result<ReservaDto>>
{
    /// <summary>
    /// ID de la reserva
    /// </summary>
    public int ReservaId { get; init; }
    
    /// <summary>
    /// ID del usuario que consulta (para validar permisos)
    /// </summary>
    public string? UsuarioId { get; init; }
    
    /// <summary>
    /// Rol del usuario que consulta
    /// </summary>
    public string? RolUsuario { get; init; }
}


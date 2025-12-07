using MediatR;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetSlotsDisponibles;

/// <summary>
/// Query para obtener los slots disponibles de una estación en una fecha
/// </summary>
public record GetSlotsDisponiblesQuery : IRequest<Result<SlotsDelDiaDto>>
{
    /// <summary>
    /// ID de la estación
    /// </summary>
    public int EstacionId { get; init; }
    
    /// <summary>
    /// Fecha para consultar disponibilidad
    /// </summary>
    public DateTime Fecha { get; init; }
    
    /// <summary>
    /// ID del servicio (para calcular duración)
    /// </summary>
    public int ServicioId { get; init; }
}


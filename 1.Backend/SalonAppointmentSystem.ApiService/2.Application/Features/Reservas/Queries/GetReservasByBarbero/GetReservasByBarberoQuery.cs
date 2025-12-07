using MediatR;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetReservasByBarbero;

/// <summary>
/// Query para obtener las reservas/agenda del barbero
/// </summary>
public record GetReservasByBarberoQuery : IRequest<Result<List<ReservaDto>>>
{
    /// <summary>
    /// ID del barbero (usuario)
    /// </summary>
    public string BarberoId { get; init; } = string.Empty;
    
    /// <summary>
    /// Fecha desde (default: hoy)
    /// </summary>
    public DateTime? FechaDesde { get; init; }
    
    /// <summary>
    /// Fecha hasta (default: 7 días adelante)
    /// </summary>
    public DateTime? FechaHasta { get; init; }
    
    /// <summary>
    /// Incluir reservas canceladas
    /// </summary>
    public bool IncluirCanceladas { get; init; } = false;
}


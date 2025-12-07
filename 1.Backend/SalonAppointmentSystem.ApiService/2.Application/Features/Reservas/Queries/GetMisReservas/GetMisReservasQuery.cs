using MediatR;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetMisReservas;

/// <summary>
/// Query para que un cliente vea sus propias reservas
/// </summary>
public record GetMisReservasQuery : IRequest<Result<List<ReservaDto>>>
{
    /// <summary>
    /// ID del usuario (cliente)
    /// </summary>
    public string UsuarioId { get; init; } = string.Empty;
    
    /// <summary>
    /// Filtrar por estado
    /// </summary>
    public EstadoReserva? Estado { get; init; }
    
    /// <summary>
    /// Solo futuras (default: true)
    /// </summary>
    public bool SoloFuturas { get; init; } = true;
    
    /// <summary>
    /// Incluir canceladas
    /// </summary>
    public bool IncluirCanceladas { get; init; } = false;
    
    /// <summary>
    /// Límite de resultados (default: 50)
    /// </summary>
    public int Limite { get; init; } = 50;
}


using MediatR;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetCitaRapida;

/// <summary>
/// Query para obtener la próxima cita rápida disponible
/// Busca el primer slot libre en cualquier estación activa
/// </summary>
public record GetCitaRapidaQuery : IRequest<Result<CitasRapidasDisponiblesDto>>
{
    /// <summary>
    /// ID del servicio a realizar
    /// </summary>
    public int ServicioId { get; init; }
    
    /// <summary>
    /// Fecha/hora desde la cual buscar (default: ahora)
    /// </summary>
    public DateTime? FechaDesde { get; init; }
    
    /// <summary>
    /// Máximo número de opciones a retornar (default: 5)
    /// </summary>
    public int MaxOpciones { get; init; } = 5;
}


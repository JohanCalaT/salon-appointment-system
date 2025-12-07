using MediatR;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.Shared.DTOs.Reservas;
using SalonAppointmentSystem.Shared.Enums;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetReservasPaged;

/// <summary>
/// Query para obtener reservas paginadas con filtros (Admin)
/// </summary>
public record GetReservasPagedQuery : IRequest<Result<PagedResult<ReservaListDto>>>
{
    /// <summary>
    /// Número de página (1-based)
    /// </summary>
    public int PageNumber { get; init; } = 1;
    
    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int PageSize { get; init; } = 10;
    
    /// <summary>
    /// Filtrar por ID de estación
    /// </summary>
    public int? EstacionId { get; init; }
    
    /// <summary>
    /// Filtrar por ID de servicio
    /// </summary>
    public int? ServicioId { get; init; }
    
    /// <summary>
    /// Filtrar por estado
    /// </summary>
    public EstadoReserva? Estado { get; init; }
    
    /// <summary>
    /// Filtrar desde fecha
    /// </summary>
    public DateTime? FechaDesde { get; init; }
    
    /// <summary>
    /// Filtrar hasta fecha
    /// </summary>
    public DateTime? FechaHasta { get; init; }
    
    /// <summary>
    /// Buscar por nombre, email o teléfono del cliente
    /// </summary>
    public string? BusquedaCliente { get; init; }
    
    /// <summary>
    /// Ordenar por campo (FechaHora, Estado, NombreCliente)
    /// </summary>
    public string OrderBy { get; init; } = "FechaHora";
    
    /// <summary>
    /// Orden descendente
    /// </summary>
    public bool OrderDescending { get; init; } = true;
}


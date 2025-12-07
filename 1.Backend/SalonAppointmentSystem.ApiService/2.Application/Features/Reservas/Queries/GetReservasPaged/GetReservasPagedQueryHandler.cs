using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetReservasPaged;

/// <summary>
/// Handler para obtener reservas paginadas
/// </summary>
public class GetReservasPagedQueryHandler
    : IRequestHandler<GetReservasPagedQuery, Result<PagedResult<ReservaListDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetReservasPagedQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<ReservaListDto>>> Handle(
        GetReservasPagedQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Reservas
            .Include(r => r.Estacion)
            .Include(r => r.Servicio)
            .AsQueryable();

        // Aplicar filtros
        query = ApplyFilters(query, request);

        // Contar total
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar ordenamiento
        query = ApplyOrdering(query, request);

        // Aplicar paginación
        var reservas = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Mapear a DTOs
        var dtos = _mapper.Map<List<ReservaListDto>>(reservas);

        var result = PagedResult<ReservaListDto>.Create(
            dtos, totalCount, request.PageNumber, request.PageSize);

        return Result<PagedResult<ReservaListDto>>.Success(result);
    }

    private static IQueryable<Reserva> ApplyFilters(
        IQueryable<Reserva> query,
        GetReservasPagedQuery request)
    {
        if (request.EstacionId.HasValue)
            query = query.Where(r => r.EstacionId == request.EstacionId.Value);

        if (request.ServicioId.HasValue)
            query = query.Where(r => r.ServicioId == request.ServicioId.Value);

        if (request.Estado.HasValue)
            query = query.Where(r => r.Estado == request.Estado.Value);

        if (request.FechaDesde.HasValue)
            query = query.Where(r => r.FechaHora >= request.FechaDesde.Value);

        if (request.FechaHasta.HasValue)
            query = query.Where(r => r.FechaHora <= request.FechaHasta.Value);

        if (!string.IsNullOrWhiteSpace(request.BusquedaCliente))
        {
            var busqueda = request.BusquedaCliente.ToLower();
            query = query.Where(r =>
                r.NombreCliente.ToLower().Contains(busqueda) ||
                r.EmailCliente.ToLower().Contains(busqueda) ||
                r.TelefonoCliente.Contains(busqueda) ||
                r.CodigoReserva.ToLower().Contains(busqueda));
        }

        return query;
    }

    private static IQueryable<Reserva> ApplyOrdering(
        IQueryable<Reserva> query,
        GetReservasPagedQuery request)
    {
        return request.OrderBy?.ToLowerInvariant() switch
        {
            "estado" => request.OrderDescending
                ? query.OrderByDescending(r => r.Estado)
                : query.OrderBy(r => r.Estado),
            "nombrecliente" => request.OrderDescending
                ? query.OrderByDescending(r => r.NombreCliente)
                : query.OrderBy(r => r.NombreCliente),
            "precio" => request.OrderDescending
                ? query.OrderByDescending(r => r.Precio)
                : query.OrderBy(r => r.Precio),
            _ => request.OrderDescending
                ? query.OrderByDescending(r => r.FechaHora)
                : query.OrderBy(r => r.FechaHora)
        };
    }
}


using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetReservasByBarbero;

/// <summary>
/// Handler para obtener la agenda del barbero
/// </summary>
public class GetReservasByBarberoQueryHandler
    : IRequestHandler<GetReservasByBarberoQuery, Result<List<ReservaDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetReservasByBarberoQueryHandler(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ReservaDto>>> Handle(
        GetReservasByBarberoQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Obtener la estación del barbero
        var estacion = await _unitOfWork.Estaciones
            .GetByBarberoIdAsync(request.BarberoId, cancellationToken);

        if (estacion == null)
        {
            return Result<List<ReservaDto>>.Failure(
                "El barbero no tiene estación asignada");
        }

        // 2. Definir rango de fechas
        var fechaDesde = request.FechaDesde?.Date ?? DateTime.UtcNow.Date;
        var fechaHasta = request.FechaHasta?.Date.AddDays(1) ?? DateTime.UtcNow.Date.AddDays(8);

        // 3. Consultar reservas
        var query = _context.Reservas
            .Include(r => r.Estacion)
            .Include(r => r.Servicio)
            .Where(r => r.EstacionId == estacion.Id)
            .Where(r => r.FechaHora >= fechaDesde && r.FechaHora < fechaHasta);

        // 4. Filtrar canceladas si es necesario
        if (!request.IncluirCanceladas)
        {
            query = query.Where(r => r.Estado != EstadoReserva.Cancelada);
        }

        // 5. Ordenar por fecha/hora
        var reservas = await query
            .OrderBy(r => r.FechaHora)
            .ToListAsync(cancellationToken);

        // 6. Mapear y retornar
        var dtos = _mapper.Map<List<ReservaDto>>(reservas);
        return Result<List<ReservaDto>>.Success(dtos);
    }
}


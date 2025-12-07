using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetMisReservas;

/// <summary>
/// Handler para obtener las reservas del cliente actual
/// </summary>
public class GetMisReservasQueryHandler
    : IRequestHandler<GetMisReservasQuery, Result<List<ReservaDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMisReservasQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<ReservaDto>>> Handle(
        GetMisReservasQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UsuarioId))
        {
            return Result<List<ReservaDto>>.Failure("Usuario no identificado");
        }

        var query = _context.Reservas
            .Include(r => r.Estacion)
            .Include(r => r.Servicio)
            .Where(r => r.UsuarioId == request.UsuarioId);

        // Filtrar por estado específico
        if (request.Estado.HasValue)
        {
            query = query.Where(r => r.Estado == request.Estado.Value);
        }
        else
        {
            // Excluir canceladas si no se piden explícitamente
            if (!request.IncluirCanceladas)
            {
                query = query.Where(r => r.Estado != EstadoReserva.Cancelada);
            }
        }

        // Solo futuras
        if (request.SoloFuturas)
        {
            query = query.Where(r => r.FechaHora >= DateTime.UtcNow);
        }

        // Ordenar: futuras primero por fecha asc, pasadas por fecha desc
        if (request.SoloFuturas)
        {
            query = query.OrderBy(r => r.FechaHora);
        }
        else
        {
            query = query.OrderByDescending(r => r.FechaHora);
        }

        // Limitar resultados
        var reservas = await query
            .Take(request.Limite)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<ReservaDto>>(reservas);
        return Result<List<ReservaDto>>.Success(dtos);
    }
}


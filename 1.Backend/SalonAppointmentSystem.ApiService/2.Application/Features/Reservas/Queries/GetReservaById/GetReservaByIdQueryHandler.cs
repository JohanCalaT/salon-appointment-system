using AutoMapper;
using MediatR;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetReservaById;

/// <summary>
/// Handler para obtener una reserva por ID con validación de permisos
/// </summary>
public class GetReservaByIdQueryHandler
    : IRequestHandler<GetReservaByIdQuery, Result<ReservaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetReservaByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ReservaDto>> Handle(
        GetReservaByIdQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Obtener reserva con detalles
        var reserva = await _unitOfWork.Reservas.GetByIdWithDetailsAsync(
            request.ReservaId, cancellationToken);

        if (reserva == null)
            return Result<ReservaDto>.Failure("Reserva no encontrada");

        // 2. Validar permisos según rol
        if (!TienePermiso(request, reserva))
        {
            return Result<ReservaDto>.Failure("No tiene permiso para ver esta reserva");
        }

        // 3. Mapear y retornar
        var dto = _mapper.Map<ReservaDto>(reserva);
        return Result<ReservaDto>.Success(dto);
    }

    private bool TienePermiso(GetReservaByIdQuery request, Domain.Entities.Reserva reserva)
    {
        // Admin puede ver cualquier reserva
        if (request.RolUsuario == "Admin")
            return true;

        // Barbero puede ver reservas de su estación
        if (request.RolUsuario == "Barbero")
        {
            // TODO: Verificar que el barbero está asignado a la estación
            // Por ahora permitimos a cualquier barbero ver cualquier reserva
            return true;
        }

        // Cliente solo puede ver sus propias reservas
        if (request.RolUsuario == "Cliente")
        {
            return reserva.UsuarioId == request.UsuarioId;
        }

        // Invitados no autenticados no pueden usar este endpoint
        // (deben usar GetByCodigoQuery)
        return false;
    }
}


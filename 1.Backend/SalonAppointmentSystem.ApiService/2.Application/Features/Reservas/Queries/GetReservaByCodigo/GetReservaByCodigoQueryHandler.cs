using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetReservaByCodigo;

/// <summary>
/// Handler para consultar reserva por código (invitados)
/// </summary>
public class GetReservaByCodigoQueryHandler
    : IRequestHandler<GetReservaByCodigoQuery, Result<ReservaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetReservaByCodigoQueryHandler> _logger;

    public GetReservaByCodigoQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetReservaByCodigoQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ReservaDto>> Handle(
        GetReservaByCodigoQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Buscar reserva por código
        var reserva = await _unitOfWork.Reservas
            .FirstOrDefaultAsync(r => r.CodigoReserva == request.Codigo.ToUpperInvariant(), 
                cancellationToken);

        if (reserva == null)
        {
            _logger.LogWarning("Intento de consulta con código inválido: {Codigo}", request.Codigo);
            return Result<ReservaDto>.Failure("Reserva no encontrada");
        }

        // 2. Verificar email
        if (!string.Equals(reserva.EmailCliente, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Intento de consulta con email incorrecto: Codigo={Codigo}, " +
                "EmailReserva={EmailReserva}, EmailIntento={EmailIntento}",
                request.Codigo, reserva.EmailCliente, request.Email);
            return Result<ReservaDto>.Failure("El email no coincide con la reserva");
        }

        // 3. Cargar detalles (estación y servicio)
        var reservaConDetalles = await _unitOfWork.Reservas
            .GetByIdWithDetailsAsync(reserva.Id, cancellationToken);

        // 4. Mapear y retornar
        var dto = _mapper.Map<ReservaDto>(reservaConDetalles ?? reserva);
        return Result<ReservaDto>.Success(dto);
    }
}


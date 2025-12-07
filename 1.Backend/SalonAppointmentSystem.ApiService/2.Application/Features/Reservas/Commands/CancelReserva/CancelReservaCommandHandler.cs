using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.CancelReserva;

/// <summary>
/// Handler para cancelar una reserva
/// </summary>
public class CancelReservaCommandHandler
    : IRequestHandler<CancelReservaCommand, Result<ReservaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReservaCacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<CancelReservaCommandHandler> _logger;

    public CancelReservaCommandHandler(
        IUnitOfWork unitOfWork,
        IReservaCacheService cacheService,
        IMapper mapper,
        ILogger<CancelReservaCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ReservaDto>> Handle(
        CancelReservaCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Obtener la reserva
        var reserva = await _unitOfWork.Reservas.GetByIdAsync(request.ReservaId, cancellationToken);

        if (reserva == null)
            return Result<ReservaDto>.Failure("Reserva no encontrada");

        // 2. Verificar email si es cancelación por código (invitado)
        if (request.EsCancelacionPorCodigo)
        {
            if (!string.Equals(reserva.EmailCliente, request.EmailVerificacion, 
                StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Intento de cancelación con email incorrecto: ReservaId={ReservaId}, " +
                    "EmailReserva={EmailReserva}, EmailIntento={EmailIntento}",
                    request.ReservaId, reserva.EmailCliente, request.EmailVerificacion);
                return Result<ReservaDto>.Failure("El email no coincide con la reserva");
            }
        }

        // 3. Verificar estado actual
        if (reserva.Estado == EstadoReserva.Cancelada)
            return Result<ReservaDto>.Failure("La reserva ya está cancelada");

        if (reserva.Estado == EstadoReserva.Completada)
            return Result<ReservaDto>.Failure("No se puede cancelar una reserva completada");

        // 4. Verificar tiempo mínimo de anticipación (excepto para Admin)
        if (request.RolCancelador != "Admin")
        {
            var tiempoMinimo = await _unitOfWork.ConfiguracionGeneral
                .GetValorEnteroAsync(ConfiguracionClaves.TiempoMinimoAnticipacionMinutos, 30, cancellationToken);

            if (reserva.FechaHora <= DateTime.UtcNow.AddMinutes(tiempoMinimo))
            {
                return Result<ReservaDto>.Failure(
                    $"No se puede cancelar con menos de {tiempoMinimo} minutos de anticipación");
            }
        }

        // 5. Cancelar la reserva
        reserva.Cancelar(request.CanceladaPor, request.MotivoCancelacion);

        _unitOfWork.Reservas.Update(reserva);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Invalidar cache
        await _cacheService.InvalidateSlotsAsync(reserva.EstacionId, reserva.FechaHora.Date);

        _logger.LogInformation(
            "Reserva cancelada: ID={ReservaId}, Codigo={Codigo}, " +
            "CanceladaPor={CanceladaPor}, Motivo={Motivo}",
            reserva.Id, reserva.CodigoReserva,
            request.CanceladaPor, request.MotivoCancelacion);

        var dto = _mapper.Map<ReservaDto>(reserva);
        return Result<ReservaDto>.Success(dto);
    }
}


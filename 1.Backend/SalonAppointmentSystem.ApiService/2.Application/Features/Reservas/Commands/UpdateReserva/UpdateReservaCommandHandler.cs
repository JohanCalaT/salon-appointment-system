using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.UpdateReserva;

/// <summary>
/// Handler para actualizar una reserva existente
/// </summary>
public class UpdateReservaCommandHandler
    : IRequestHandler<UpdateReservaCommand, Result<ReservaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReservaLockService _lockService;
    private readonly IReservaCacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateReservaCommandHandler> _logger;

    public UpdateReservaCommandHandler(
        IUnitOfWork unitOfWork,
        IReservaLockService lockService,
        IReservaCacheService cacheService,
        IMapper mapper,
        ILogger<UpdateReservaCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _lockService = lockService;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ReservaDto>> Handle(
        UpdateReservaCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Obtener la reserva
        var reserva = await _unitOfWork.Reservas.GetByIdAsync(request.ReservaId, cancellationToken);

        if (reserva == null)
            return Result<ReservaDto>.Failure("Reserva no encontrada");

        // 2. Verificar estado actual
        if (reserva.Estado == EstadoReserva.Cancelada)
            return Result<ReservaDto>.Failure("No se puede actualizar una reserva cancelada");

        if (reserva.Estado == EstadoReserva.Completada)
            return Result<ReservaDto>.Failure("No se puede actualizar una reserva completada");

        var estacionIdOriginal = reserva.EstacionId;
        var fechaHoraOriginal = reserva.FechaHora;
        var cambiaHorario = request.FechaHora.HasValue || request.EstacionId.HasValue;

        // 3. Si cambia horario o estación, verificar disponibilidad
        if (cambiaHorario)
        {
            var nuevaEstacionId = request.EstacionId ?? reserva.EstacionId;
            var nuevaFechaHora = request.FechaHora ?? reserva.FechaHora;

            // Verificar que la estación existe
            var estacion = await _unitOfWork.Estaciones.GetByIdAsync(nuevaEstacionId, cancellationToken);
            if (estacion == null || !estacion.Activa)
                return Result<ReservaDto>.Failure("Estación no encontrada o inactiva");

            // Adquirir lock
            await using var lockHandle = await _lockService.AcquireLockAsync(
                nuevaEstacionId, nuevaFechaHora, reserva.DuracionMinutos);

            if (lockHandle == null)
                return Result<ReservaDto>.Failure(
                    "El horario está siendo reservado. Intente nuevamente.");

            // Verificar solapamiento (excluyendo la reserva actual)
            var haySolapamiento = await _unitOfWork.Reservas.ExisteSolapamientoAsync(
                nuevaEstacionId, nuevaFechaHora, reserva.DuracionMinutos,
                reserva.Id, cancellationToken);

            if (haySolapamiento)
                return Result<ReservaDto>.Failure("El nuevo horario no está disponible");

            // Aplicar cambios de horario
            if (request.EstacionId.HasValue)
                reserva.EstacionId = request.EstacionId.Value;

            if (request.FechaHora.HasValue)
                reserva.FechaHora = request.FechaHora.Value;
        }

        // 4. Aplicar cambios de datos del cliente
        if (!string.IsNullOrEmpty(request.NombreCliente))
            reserva.NombreCliente = request.NombreCliente;

        if (!string.IsNullOrEmpty(request.EmailCliente))
            reserva.EmailCliente = request.EmailCliente;

        if (!string.IsNullOrEmpty(request.TelefonoCliente))
            reserva.TelefonoCliente = request.TelefonoCliente;

        // 5. Aplicar cambio de estado (con validaciones)
        if (request.Estado.HasValue && request.Estado.Value != reserva.Estado)
        {
            var nuevoEstado = request.Estado.Value;

            // Solo Admin puede cambiar a cualquier estado
            if (request.RolActualizador != "Admin")
            {
                // Barbero solo puede: Pendiente -> Confirmada, Confirmada -> Completada
                if (reserva.Estado == EstadoReserva.Pendiente && 
                    nuevoEstado != EstadoReserva.Confirmada)
                {
                    return Result<ReservaDto>.Failure(
                        "Solo puede confirmar una reserva pendiente");
                }
            }

            reserva.Estado = nuevoEstado;
        }

        // 6. Guardar cambios
        _unitOfWork.Reservas.Update(reserva);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Invalidar cache si cambió horario o estación
        if (cambiaHorario)
        {
            await _cacheService.InvalidateSlotsAsync(estacionIdOriginal, fechaHoraOriginal.Date);
            await _cacheService.InvalidateSlotsAsync(reserva.EstacionId, reserva.FechaHora.Date);
        }

        _logger.LogInformation(
            "Reserva actualizada: ID={ReservaId}, Por={ActualizadaPor}",
            reserva.Id, request.ActualizadaPor);

        var dto = _mapper.Map<ReservaDto>(reserva);
        return Result<ReservaDto>.Success(dto);
    }
}


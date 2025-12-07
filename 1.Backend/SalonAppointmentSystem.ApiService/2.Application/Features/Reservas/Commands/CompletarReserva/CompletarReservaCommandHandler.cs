using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.CompletarReserva;

/// <summary>
/// Handler para marcar una reserva como completada
/// </summary>
public class CompletarReservaCommandHandler
    : IRequestHandler<CompletarReservaCommand, Result<ReservaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CompletarReservaCommandHandler> _logger;

    public CompletarReservaCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CompletarReservaCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ReservaDto>> Handle(
        CompletarReservaCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Obtener la reserva
        var reserva = await _unitOfWork.Reservas.GetByIdAsync(request.ReservaId, cancellationToken);

        if (reserva == null)
            return Result<ReservaDto>.Failure("Reserva no encontrada");

        // 2. Verificar estado actual
        if (reserva.Estado == EstadoReserva.Completada)
            return Result<ReservaDto>.Failure("La reserva ya está completada");

        if (reserva.Estado == EstadoReserva.Cancelada)
            return Result<ReservaDto>.Failure("No se puede completar una reserva cancelada");

        // 3. Verificar que la fecha/hora ya pasó o es cercana (tolerancia de 15 min antes)
        var tolerancia = TimeSpan.FromMinutes(15);
        if (reserva.FechaHora > DateTime.UtcNow.Add(tolerancia))
        {
            return Result<ReservaDto>.Failure(
                "No se puede completar una reserva antes de su hora programada");
        }

        // 4. Completar la reserva
        reserva.Completar();

        _unitOfWork.Reservas.Update(reserva);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. TODO: Agregar puntos al usuario si está registrado
        if (!string.IsNullOrEmpty(reserva.UsuarioId) && reserva.PuntosGanados > 0)
        {
            _logger.LogInformation(
                "Puntos acumulados para usuario {UsuarioId}: {Puntos} puntos",
                reserva.UsuarioId, reserva.PuntosGanados);
            // Aquí se podría agregar la lógica de puntos de fidelidad
        }

        _logger.LogInformation(
            "Reserva completada: ID={ReservaId}, Codigo={Codigo}, " +
            "CompletadaPor={CompletadaPor}, Puntos={Puntos}",
            reserva.Id, reserva.CodigoReserva,
            request.CompletadaPor, reserva.PuntosGanados);

        var dto = _mapper.Map<ReservaDto>(reserva);
        return Result<ReservaDto>.Success(dto);
    }
}


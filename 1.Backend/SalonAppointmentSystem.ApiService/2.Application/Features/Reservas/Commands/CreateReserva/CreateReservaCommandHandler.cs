using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SalonAppointmentSystem.ApiService.Application.Behaviors;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.CreateReserva;

/// <summary>
/// Handler para crear una nueva reserva con lock distribuido
/// </summary>
public class CreateReservaCommandHandler
    : IRequestHandler<CreateReservaCommand, Result<ReservaDto>>, ITransactionalCommand
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReservaLockService _lockService;
    private readonly IReservaCacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateReservaCommandHandler> _logger;

    public CreateReservaCommandHandler(
        IUnitOfWork unitOfWork,
        IReservaLockService lockService,
        IReservaCacheService cacheService,
        IMapper mapper,
        ILogger<CreateReservaCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _lockService = lockService;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ReservaDto>> Handle(
        CreateReservaCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Obtener el servicio para conocer la duración y precio
        var servicio = await _unitOfWork.Servicios.GetByIdAsync(request.ServicioId, cancellationToken);
        if (servicio == null || !servicio.Activo)
            return Result<ReservaDto>.Failure("Servicio no encontrado o inactivo");

        // 2. Verificar que la estación existe y está activa
        var estacion = await _unitOfWork.Estaciones.GetByIdAsync(request.EstacionId, cancellationToken);
        if (estacion == null || !estacion.Activa)
            return Result<ReservaDto>.Failure("Estación no encontrada o inactiva");

        // 3. Verificar configuración de anticipación mínima
        var tiempoMinimo = await _unitOfWork.ConfiguracionGeneral
            .GetValorEnteroAsync(ConfiguracionClaves.TiempoMinimoAnticipacionMinutos, 30, cancellationToken);

        if (request.FechaHora < DateTime.UtcNow.AddMinutes(tiempoMinimo))
            return Result<ReservaDto>.Failure(
                $"Las reservas deben hacerse con al menos {tiempoMinimo} minutos de anticipación");

        // 4. Verificar configuración de días máximos de anticipación
        var diasMaximo = await _unitOfWork.ConfiguracionGeneral
            .GetValorEnteroAsync(ConfiguracionClaves.DiasMaximoReservaFutura, 30, cancellationToken);

        if (request.FechaHora > DateTime.UtcNow.AddDays(diasMaximo))
            return Result<ReservaDto>.Failure(
                $"Las reservas no pueden hacerse con más de {diasMaximo} días de anticipación");

        // 5. ADQUIRIR LOCK DISTRIBUIDO
        await using var lockHandle = await _lockService.AcquireLockAsync(
            request.EstacionId,
            request.FechaHora,
            servicio.DuracionMinutos);

        if (lockHandle == null)
        {
            _logger.LogWarning(
                "No se pudo adquirir lock para reserva: Est={EstacionId}, Hora={Hora}",
                request.EstacionId, request.FechaHora);
            return Result<ReservaDto>.Failure(
                "El horario está siendo reservado por otro usuario. Intente nuevamente.");
        }

        // 6. VERIFICAR HORARIO DE ATENCIÓN (dentro del lock)
        var horarioResult = await ValidarHorarioAtencionAsync(
            estacion, request.FechaHora, servicio.DuracionMinutos, cancellationToken);

        if (!horarioResult.IsSuccess)
            return Result<ReservaDto>.Failure(horarioResult.Error!);

        // 7. VERIFICAR DISPONIBILIDAD (dentro del lock)
        var haySolapamiento = await _unitOfWork.Reservas.ExisteSolapamientoAsync(
            request.EstacionId, request.FechaHora, servicio.DuracionMinutos,
            cancellationToken: cancellationToken);

        if (haySolapamiento)
        {
            _logger.LogInformation(
                "Reserva rechazada por solapamiento: Est={EstacionId}, Hora={Hora}",
                request.EstacionId, request.FechaHora);
            return Result<ReservaDto>.Failure("El horario seleccionado ya no está disponible");
        }

        // 8. Calcular puntos (solo para usuarios registrados)
        var puntosGanados = request.UsuarioId != null ? servicio.PuntosQueOtorga : 0;

        // 9. CREAR LA RESERVA
        var reserva = new Reserva
        {
            EstacionId = request.EstacionId,
            ServicioId = request.ServicioId,
            UsuarioId = request.UsuarioId,
            CreadaPor = request.CreadaPor,
            RolCreador = request.RolCreador,
            NombreCliente = request.NombreCliente,
            EmailCliente = request.EmailCliente,
            TelefonoCliente = request.TelefonoCliente,
            FechaHora = request.FechaHora,
            DuracionMinutos = servicio.DuracionMinutos,
            Precio = servicio.Precio,
            PuntosGanados = puntosGanados,
            Estado = EstadoReserva.Confirmada,
            TipoReserva = request.TipoReserva,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.Reservas.AddAsync(reserva, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 10. INVALIDAR CACHE
        await _cacheService.InvalidateSlotsAsync(request.EstacionId, request.FechaHora.Date);

        _logger.LogInformation(
            "Reserva creada: ID={ReservaId}, Codigo={Codigo}, Est={EstacionId}, " +
            "Cliente={Cliente}, Hora={Hora}, Puntos={Puntos}",
            reserva.Id, reserva.CodigoReserva, request.EstacionId,
            request.NombreCliente, request.FechaHora, puntosGanados);

        // 11. Mapear y retornar
        var dto = _mapper.Map<ReservaDto>(reserva);
        return Result<ReservaDto>.Success(dto);
    }

    private async Task<Result> ValidarHorarioAtencionAsync(
        Estacion estacion, DateTime fechaHora, int duracionMinutos,
        CancellationToken cancellationToken)
    {
        var horario = await _unitOfWork.ConfiguracionHorarios
            .GetHorarioParaEstacionYFechaAsync(
                estacion.Id, fechaHora, estacion.UsaHorarioGenerico, cancellationToken);

        if (horario == null)
            return Result.Failure("No hay horario configurado para esta fecha");

        if (horario.Tipo == TipoHorario.Bloqueado)
            return Result.Failure("Este día está bloqueado para la estación seleccionada");

        var horaInicio = TimeSpan.FromTicks(fechaHora.TimeOfDay.Ticks);
        var horaFin = horaInicio.Add(TimeSpan.FromMinutes(duracionMinutos));

        if (horaInicio < horario.HoraInicio || horaFin > horario.HoraFin)
            return Result.Failure(
                $"El horario debe estar entre {horario.HoraInicio:hh\\:mm} y {horario.HoraFin:hh\\:mm}");

        return Result.Success();
    }
}


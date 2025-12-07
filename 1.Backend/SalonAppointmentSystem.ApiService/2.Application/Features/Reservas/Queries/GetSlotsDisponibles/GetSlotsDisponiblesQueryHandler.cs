using MediatR;
using Microsoft.Extensions.Logging;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.Shared.Enums;
using CacheSlotDto = SalonAppointmentSystem.ApiService.Application.Common.Interfaces.SlotDisponibleDto;
using SlotDisponibleDto = SalonAppointmentSystem.Shared.DTOs.Reservas.SlotDisponibleDto;
using SlotsDelDiaDto = SalonAppointmentSystem.Shared.DTOs.Reservas.SlotsDelDiaDto;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetSlotsDisponibles;

/// <summary>
/// Handler para obtener slots disponibles con cache
/// </summary>
public class GetSlotsDisponiblesQueryHandler
    : IRequestHandler<GetSlotsDisponiblesQuery, Result<SlotsDelDiaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReservaCacheService _cacheService;
    private readonly ILogger<GetSlotsDisponiblesQueryHandler> _logger;
    private const int IntervaloSlotMinutos = 15;

    public GetSlotsDisponiblesQueryHandler(
        IUnitOfWork unitOfWork,
        IReservaCacheService cacheService,
        ILogger<GetSlotsDisponiblesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<SlotsDelDiaDto>> Handle(
        GetSlotsDisponiblesQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Validar servicio
        var servicio = await _unitOfWork.Servicios.GetByIdAsync(request.ServicioId, cancellationToken);
        if (servicio == null || !servicio.Activo)
            return Result<SlotsDelDiaDto>.Failure("Servicio no encontrado o inactivo");

        // 2. Validar estación
        var estacion = await _unitOfWork.Estaciones.GetByIdAsync(request.EstacionId, cancellationToken);
        if (estacion == null || !estacion.Activa)
            return Result<SlotsDelDiaDto>.Failure("Estación no encontrada o inactiva");

        if (!estacion.PuedeRecibirReservas)
            return Result<SlotsDelDiaDto>.Failure("La estación no tiene barbero asignado");

        // 3. Intentar obtener del cache
        var cachedSlots = await _cacheService.GetSlotsDisponiblesAsync(
            request.EstacionId, request.Fecha.Date, request.ServicioId);

        if (cachedSlots != null)
        {
            _logger.LogDebug("Slots obtenidos del cache: Est={EstacionId}, Fecha={Fecha}",
                request.EstacionId, request.Fecha.Date);

            return Result<SlotsDelDiaDto>.Success(new SlotsDelDiaDto
            {
                Fecha = request.Fecha.Date,
                Slots = cachedSlots.Select(s => new SlotDisponibleDto
                {
                    FechaHora = s.FechaHora,
                    HoraFormateada = s.HoraFormateada,
                    Disponible = s.Disponible
                }).ToList()
            });
        }

        // 4. Obtener horario de atención
        var horario = await _unitOfWork.ConfiguracionHorarios
            .GetHorarioParaEstacionYFechaAsync(
                request.EstacionId, request.Fecha, estacion.UsaHorarioGenerico, cancellationToken);

        if (horario == null || horario.Tipo == TipoHorario.Bloqueado)
        {
            var emptyResult = new SlotsDelDiaDto
            {
                Fecha = request.Fecha.Date,
                Slots = new List<SlotDisponibleDto>()
            };
            return Result<SlotsDelDiaDto>.Success(emptyResult);
        }

        // 5. Obtener reservas del día
        var fechaInicio = request.Fecha.Date;
        var fechaFin = request.Fecha.Date.AddDays(1).AddTicks(-1);
        var reservasDelDia = await _unitOfWork.Reservas
            .GetByEstacionYFechaAsync(request.EstacionId, fechaInicio, fechaFin, cancellationToken);

        // 6. Calcular slots disponibles
        var slots = CalcularSlots(
            request.Fecha.Date, horario.HoraInicio, horario.HoraFin,
            servicio.DuracionMinutos, reservasDelDia);

        // 7. Guardar en cache
        var slotsCache = slots.Select(s => new Common.Interfaces.SlotDisponibleDto
        {
            FechaHora = s.FechaHora,
            HoraFormateada = s.HoraFormateada,
            Disponible = s.Disponible
        }).ToList();

        await _cacheService.SetSlotsDisponiblesAsync(
            request.EstacionId, request.Fecha.Date, request.ServicioId, slotsCache);

        var result = new SlotsDelDiaDto
        {
            Fecha = request.Fecha.Date,
            Slots = slots
        };

        return Result<SlotsDelDiaDto>.Success(result);
    }

    private List<SlotDisponibleDto> CalcularSlots(
        DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin,
        int duracionMinutos, IEnumerable<Domain.Entities.Reserva> reservas)
    {
        var slots = new List<SlotDisponibleDto>();
        var ahora = DateTime.UtcNow;
        var horaActual = horaInicio;

        while (horaActual.Add(TimeSpan.FromMinutes(duracionMinutos)) <= horaFin)
        {
            var fechaHoraSlot = fecha.Add(horaActual);
            var fechaHoraFinSlot = fechaHoraSlot.AddMinutes(duracionMinutos);

            // Verificar si está en el futuro
            var esFuturo = fechaHoraSlot > ahora;

            // Verificar solapamiento con reservas existentes
            var estaOcupado = reservas.Any(r =>
                r.Estado != EstadoReserva.Cancelada &&
                r.FechaHora < fechaHoraFinSlot &&
                r.FechaHora.AddMinutes(r.DuracionMinutos) > fechaHoraSlot);

            string? razonNoDisponible = null;
            if (!esFuturo)
                razonNoDisponible = "Horario pasado";
            else if (estaOcupado)
                razonNoDisponible = "Horario ocupado";

            slots.Add(new SlotDisponibleDto
            {
                FechaHora = fechaHoraSlot,
                HoraFormateada = horaActual.ToString(@"hh\:mm"),
                Disponible = esFuturo && !estaOcupado,
                RazonNoDisponible = razonNoDisponible
            });

            horaActual = horaActual.Add(TimeSpan.FromMinutes(IntervaloSlotMinutos));
        }

        return slots;
    }
}


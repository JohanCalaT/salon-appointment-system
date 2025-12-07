using MediatR;
using Microsoft.EntityFrameworkCore;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.Shared.DTOs.Reservas;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetCitaRapida;

/// <summary>
/// Handler para buscar la próxima cita disponible en cualquier estación
/// </summary>
public class GetCitaRapidaQueryHandler
    : IRequestHandler<GetCitaRapidaQuery, Result<CitasRapidasDisponiblesDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private const int IntervaloSlotMinutos = 15;
    private const int DiasABuscar = 7; // Buscar hasta 7 días adelante

    public GetCitaRapidaQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CitasRapidasDisponiblesDto>> Handle(
        GetCitaRapidaQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Validar servicio
        var servicio = await _unitOfWork.Servicios.GetByIdAsync(request.ServicioId, cancellationToken);
        if (servicio == null || !servicio.Activo)
            return Result<CitasRapidasDisponiblesDto>.Failure("Servicio no encontrado o inactivo");

        // 2. Obtener estaciones activas con barbero
        var estaciones = await _unitOfWork.Estaciones.GetActivasAsync(cancellationToken);
        var estacionesDisponibles = estaciones.Where(e => e.PuedeRecibirReservas).ToList();

        if (!estacionesDisponibles.Any())
        {
            return Result<CitasRapidasDisponiblesDto>.Success(new CitasRapidasDisponiblesDto
            {
                Opciones = new List<CitaRapidaDto>()
            });
        }

        var fechaDesde = request.FechaDesde ?? DateTime.UtcNow;
        var opciones = new List<CitaRapidaDto>();

        // 3. Para cada estación, buscar el primer slot disponible
        foreach (var estacion in estacionesDisponibles)
        {
            var primerSlot = await BuscarPrimerSlotDisponibleAsync(
                estacion, servicio.DuracionMinutos, fechaDesde, cancellationToken);

            if (primerSlot.HasValue)
            {
                // Necesitamos obtener el nombre del barbero
                var estacionCompleta = await _unitOfWork.Estaciones.GetByIdAsync(estacion.Id, cancellationToken);
                
                opciones.Add(new CitaRapidaDto
                {
                    EstacionId = estacion.Id,
                    EstacionNombre = estacion.Nombre,
                    BarberoNombre = estacionCompleta?.Barbero?.NombreCompleto,
                    FechaHora = primerSlot.Value,
                    TiempoEsperaMinutos = (int)(primerSlot.Value - fechaDesde).TotalMinutes
                });
            }
        }

        // 4. Ordenar por tiempo de espera y tomar las mejores opciones
        var mejoresOpciones = opciones
            .OrderBy(o => o.TiempoEsperaMinutos)
            .Take(request.MaxOpciones)
            .ToList();

        return Result<CitasRapidasDisponiblesDto>.Success(new CitasRapidasDisponiblesDto
        {
            Opciones = mejoresOpciones
        });
    }

    private async Task<DateTime?> BuscarPrimerSlotDisponibleAsync(
        Estacion estacion, int duracionMinutos, DateTime desde,
        CancellationToken cancellationToken)
    {
        var fechaActual = desde.Date;
        var fechaLimite = desde.AddDays(DiasABuscar).Date;

        while (fechaActual <= fechaLimite)
        {
            // Obtener horario del día
            var horario = await _unitOfWork.ConfiguracionHorarios
                .GetHorarioParaEstacionYFechaAsync(
                    estacion.Id, fechaActual, estacion.UsaHorarioGenerico, cancellationToken);

            if (horario != null && horario.Tipo != TipoHorario.Bloqueado)
            {
                // Obtener reservas del día
                var fechaInicio = fechaActual.Date;
                var fechaFin = fechaActual.Date.AddDays(1).AddTicks(-1);
                var reservas = await _unitOfWork.Reservas
                    .GetByEstacionYFechaAsync(estacion.Id, fechaInicio, fechaFin, cancellationToken);

                // Buscar primer slot libre
                var horaInicio = horario.HoraInicio;
                
                // Si es hoy, empezar desde la hora actual
                if (fechaActual.Date == desde.Date)
                {
                    var horaActualTimeSpan = desde.TimeOfDay;
                    // Redondear al siguiente slot de 15 min
                    var minutosRedondeados = ((int)Math.Ceiling(horaActualTimeSpan.TotalMinutes / IntervaloSlotMinutos)) * IntervaloSlotMinutos;
                    var horaRedondeada = TimeSpan.FromMinutes(minutosRedondeados);
                    
                    if (horaRedondeada > horaInicio)
                        horaInicio = horaRedondeada;
                }

                while (horaInicio.Add(TimeSpan.FromMinutes(duracionMinutos)) <= horario.HoraFin)
                {
                    var fechaHoraSlot = fechaActual.Add(horaInicio);
                    var fechaHoraFinSlot = fechaHoraSlot.AddMinutes(duracionMinutos);

                    // Verificar que sea futuro
                    if (fechaHoraSlot > desde)
                    {
                        // Verificar disponibilidad
                        var estaOcupado = reservas.Any(r =>
                            r.Estado != EstadoReserva.Cancelada &&
                            r.FechaHora < fechaHoraFinSlot &&
                            r.FechaHora.AddMinutes(r.DuracionMinutos) > fechaHoraSlot);

                        if (!estaOcupado)
                            return fechaHoraSlot;
                    }

                    horaInicio = horaInicio.Add(TimeSpan.FromMinutes(IntervaloSlotMinutos));
                }
            }

            fechaActual = fechaActual.AddDays(1);
        }

        return null;
    }
}


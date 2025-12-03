using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence;
using SalonAppointmentSystem.Shared.DTOs.Horarios;
using SalonAppointmentSystem.Shared.Enums;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Services;

/// <summary>
/// Servicio para gestión de horarios
/// </summary>
public class HorarioService : IHorarioService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguracionHorarioRepository _horarioRepository;
    private readonly IEstacionRepository _estacionRepository;
    private readonly ILogger<HorarioService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HorarioService(
        ApplicationDbContext context,
        IConfiguracionHorarioRepository horarioRepository,
        IEstacionRepository estacionRepository,
        ILogger<HorarioService> logger,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _horarioRepository = horarioRepository;
        _estacionRepository = estacionRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    #region Horarios Globales

    public async Task<ApiResponse<HorarioSemanalDto>> GetHorarioGlobalAsync(CancellationToken cancellationToken = default)
    {
        var horarios = await _horarioRepository.GetHorariosGlobalesSemanaAsync(cancellationToken);
        var dto = BuildHorarioSemanalDto(null, null, false, horarios.ToList());
        return ApiResponse<HorarioSemanalDto>.Ok(dto);
    }

    public async Task<ApiResponse<HorarioSemanalDto>> ConfigurarHorarioGlobalAsync(
        ConfigurarHorarioSemanalRequest request,
        CancellationToken cancellationToken = default)
    {
        // Eliminar horarios globales existentes
        var horariosExistentes = await _context.ConfiguracionHorarios
            .Where(h => h.EstacionId == null && h.Tipo == TipoHorario.Regular)
            .ToListAsync(cancellationToken);

        _context.ConfiguracionHorarios.RemoveRange(horariosExistentes);

        // Crear nuevos horarios
        foreach (var dia in request.Dias.Where(d => d.Trabaja && d.HoraInicio.HasValue && d.HoraFin.HasValue))
        {
            var horario = new ConfiguracionHorario
            {
                EstacionId = null,
                DiaSemana = dia.DiaSemana,
                HoraInicio = dia.HoraInicio!.Value,
                HoraFin = dia.HoraFin!.Value,
                Tipo = TipoHorario.Regular,
                Activo = true
            };
            await _horarioRepository.AddAsync(horario, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Horario global configurado");

        return await GetHorarioGlobalAsync(cancellationToken);
    }

    #endregion

    #region Horarios por Estación

    public async Task<ApiResponse<HorarioSemanalDto>> GetHorarioEstacionAsync(
        int estacionId,
        CancellationToken cancellationToken = default)
    {
        var estacion = await _estacionRepository.GetByIdAsync(estacionId, cancellationToken);
        if (estacion == null)
        {
            return ApiResponse<HorarioSemanalDto>.Fail("Estación no encontrada");
        }

        List<ConfiguracionHorario> horarios;
        if (estacion.UsaHorarioGenerico)
        {
            // Usar horarios globales
            horarios = (await _horarioRepository.GetHorariosGlobalesSemanaAsync(cancellationToken)).ToList();
        }
        else
        {
            // Usar horarios personalizados
            horarios = (await _horarioRepository.GetHorariosSemanaByEstacionAsync(estacionId, cancellationToken)).ToList();
        }

        // Obtener horarios especiales y bloqueados
        var fechaDesde = DateTime.UtcNow.Date;
        var fechaHasta = fechaDesde.AddMonths(3);

        var especiales = await _horarioRepository.GetHorariosEspecialesByEstacionAsync(
            estacionId, fechaDesde, fechaHasta, cancellationToken);
        var bloqueados = await _horarioRepository.GetDiasBloqueadosByEstacionAsync(
            estacionId, fechaDesde, fechaHasta, cancellationToken);

        var dto = BuildHorarioSemanalDto(estacionId, estacion.Nombre, estacion.UsaHorarioGenerico, horarios);
        dto.HorariosEspeciales = especiales.Select(MapToHorarioDto).ToList();
        dto.DiasBloqueados = bloqueados.Select(MapToHorarioDto).ToList();

        return ApiResponse<HorarioSemanalDto>.Ok(dto);
    }

    public async Task<ApiResponse<HorarioSemanalDto>> ConfigurarHorarioEstacionAsync(
        int estacionId,
        ConfigurarHorarioSemanalRequest request,
        CancellationToken cancellationToken = default)
    {
        var estacion = await _estacionRepository.GetByIdAsync(estacionId, cancellationToken);
        if (estacion == null)
        {
            return ApiResponse<HorarioSemanalDto>.Fail("Estación no encontrada");
        }

        // Eliminar horarios personalizados existentes
        var horariosExistentes = await _context.ConfiguracionHorarios
            .Where(h => h.EstacionId == estacionId && h.Tipo == TipoHorario.Regular)
            .ToListAsync(cancellationToken);

        _context.ConfiguracionHorarios.RemoveRange(horariosExistentes);

        // Crear nuevos horarios
        foreach (var dia in request.Dias.Where(d => d.Trabaja && d.HoraInicio.HasValue && d.HoraFin.HasValue))
        {
            var horario = new ConfiguracionHorario
            {
                EstacionId = estacionId,
                DiaSemana = dia.DiaSemana,
                HoraInicio = dia.HoraInicio!.Value,
                HoraFin = dia.HoraFin!.Value,
                Tipo = TipoHorario.Regular,
                Activo = true
            };
            await _horarioRepository.AddAsync(horario, cancellationToken);
        }

        // Cambiar a horario personalizado
        estacion.UsaHorarioGenerico = false;
        _estacionRepository.Update(estacion);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Horario personalizado configurado para estación {Id}", estacionId);

        return await GetHorarioEstacionAsync(estacionId, cancellationToken);
    }

    public async Task<ApiResponse<HorarioSemanalDto>> CambiarTipoHorarioAsync(
        int estacionId,
        CambiarTipoHorarioRequest request,
        CancellationToken cancellationToken = default)
    {
        var estacion = await _estacionRepository.GetByIdAsync(estacionId, cancellationToken);
        if (estacion == null)
        {
            return ApiResponse<HorarioSemanalDto>.Fail("Estación no encontrada");
        }

        // Si cambia a personalizado y quiere copiar horarios globales
        if (!request.UsaHorarioGenerico && request.CopiarHorariosGlobales && estacion.UsaHorarioGenerico)
        {
            var horariosGlobales = await _horarioRepository.GetHorariosGlobalesSemanaAsync(cancellationToken);
            foreach (var global in horariosGlobales)
            {
                var copia = new ConfiguracionHorario
                {
                    EstacionId = estacionId,
                    DiaSemana = global.DiaSemana,
                    HoraInicio = global.HoraInicio,
                    HoraFin = global.HoraFin,
                    Tipo = TipoHorario.Regular,
                    Activo = true
                };
                await _horarioRepository.AddAsync(copia, cancellationToken);
            }
        }

        estacion.UsaHorarioGenerico = request.UsaHorarioGenerico;
        _estacionRepository.Update(estacion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var tipo = request.UsaHorarioGenerico ? "genérico" : "personalizado";
        _logger.LogInformation("Estación {Id} cambiada a horario {Tipo}", estacionId, tipo);

        return await GetHorarioEstacionAsync(estacionId, cancellationToken);
    }

    #endregion

    #region Horarios Especiales y Días Bloqueados

    public async Task<ApiResponse<HorarioDto>> CrearHorarioEspecialAsync(
        int estacionId,
        HorarioEspecialRequest request,
        CancellationToken cancellationToken = default)
    {
        var estacion = await _estacionRepository.GetByIdAsync(estacionId, cancellationToken);
        if (estacion == null)
        {
            return ApiResponse<HorarioDto>.Fail("Estación no encontrada");
        }

        // Validar tipo
        if (request.Tipo == TipoHorario.Regular)
        {
            return ApiResponse<HorarioDto>.Fail("Use ConfigurarHorarioEstacion para horarios regulares");
        }

        // Validar horas para horarios especiales
        if (request.Tipo == TipoHorario.Especial && (!request.HoraInicio.HasValue || !request.HoraFin.HasValue))
        {
            return ApiResponse<HorarioDto>.Fail("Los horarios especiales requieren hora de inicio y fin");
        }

        // Verificar solapamiento
        var existeSolapamiento = await _horarioRepository.ExisteHorarioEspecialEnFechasAsync(
            estacionId, request.FechaDesde, request.FechaHasta, request.Tipo, null, cancellationToken);

        if (existeSolapamiento)
        {
            return ApiResponse<HorarioDto>.Fail("Ya existe un horario especial o bloqueo en esas fechas");
        }

        var horario = new ConfiguracionHorario
        {
            EstacionId = estacionId,
            DiaSemana = request.FechaDesde.DayOfWeek,
            HoraInicio = request.HoraInicio ?? TimeSpan.Zero,
            HoraFin = request.HoraFin ?? TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)),
            Tipo = request.Tipo,
            FechaVigenciaDesde = request.FechaDesde,
            FechaVigenciaHasta = request.FechaHasta,
            Descripcion = request.Descripcion,
            Activo = true
        };

        await _horarioRepository.AddAsync(horario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Horario {Tipo} creado para estación {Id}: {Desde} - {Hasta}",
            request.Tipo, estacionId, request.FechaDesde, request.FechaHasta);

        return ApiResponse<HorarioDto>.Ok(MapToHorarioDto(horario), "Horario especial creado correctamente");
    }

    public async Task<ApiResponse<HorarioDto>> ActualizarHorarioEspecialAsync(
        int horarioId,
        UpdateHorarioEspecialRequest request,
        CancellationToken cancellationToken = default)
    {
        var horario = await _horarioRepository.GetByIdAsync(horarioId, cancellationToken);
        if (horario == null)
        {
            return ApiResponse<HorarioDto>.Fail("Horario no encontrado");
        }

        if (horario.Tipo == TipoHorario.Regular)
        {
            return ApiResponse<HorarioDto>.Fail("No se puede actualizar un horario regular con este método");
        }

        // Verificar solapamiento excluyendo el actual
        if (horario.EstacionId.HasValue)
        {
            var existeSolapamiento = await _horarioRepository.ExisteHorarioEspecialEnFechasAsync(
                horario.EstacionId.Value, request.FechaDesde, request.FechaHasta, request.Tipo, horarioId, cancellationToken);

            if (existeSolapamiento)
            {
                return ApiResponse<HorarioDto>.Fail("Ya existe un horario especial o bloqueo en esas fechas");
            }
        }

        horario.Tipo = request.Tipo;
        horario.FechaVigenciaDesde = request.FechaDesde;
        horario.FechaVigenciaHasta = request.FechaHasta;
        horario.HoraInicio = request.HoraInicio ?? TimeSpan.Zero;
        horario.HoraFin = request.HoraFin ?? TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59));
        horario.Descripcion = request.Descripcion;
        horario.Activo = request.Activo;

        _horarioRepository.Update(horario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Horario especial {Id} actualizado", horarioId);
        return ApiResponse<HorarioDto>.Ok(MapToHorarioDto(horario), "Horario actualizado correctamente");
    }

    public async Task<ApiResponse<bool>> EliminarHorarioEspecialAsync(
        int horarioId,
        CancellationToken cancellationToken = default)
    {
        var horario = await _horarioRepository.GetByIdAsync(horarioId, cancellationToken);
        if (horario == null)
        {
            return ApiResponse<bool>.Fail("Horario no encontrado");
        }

        if (horario.Tipo == TipoHorario.Regular)
        {
            return ApiResponse<bool>.Fail("No se puede eliminar un horario regular con este método");
        }

        _horarioRepository.Delete(horario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Horario especial {Id} eliminado", horarioId);
        return ApiResponse<bool>.Ok(true, "Horario eliminado correctamente");
    }

    public async Task<ApiResponse<IReadOnlyList<HorarioDto>>> GetHorariosEspecialesAsync(
        int estacionId,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var horarios = await _horarioRepository.GetHorariosEspecialesByEstacionAsync(
            estacionId, desde, hasta, cancellationToken);
        var dtos = horarios.Select(MapToHorarioDto).ToList();
        return ApiResponse<IReadOnlyList<HorarioDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<IReadOnlyList<HorarioDto>>> GetDiasBloqueadosAsync(
        int estacionId,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var horarios = await _horarioRepository.GetDiasBloqueadosByEstacionAsync(
            estacionId, desde, hasta, cancellationToken);
        var dtos = horarios.Select(MapToHorarioDto).ToList();
        return ApiResponse<IReadOnlyList<HorarioDto>>.Ok(dtos);
    }

    #endregion

    #region Consultas de Disponibilidad

    public async Task<ApiResponse<HorarioDto?>> GetHorarioEfectivoAsync(
        int estacionId,
        DateTime fecha,
        CancellationToken cancellationToken = default)
    {
        var estacion = await _estacionRepository.GetByIdAsync(estacionId, cancellationToken);
        if (estacion == null)
        {
            return ApiResponse<HorarioDto?>.Fail("Estación no encontrada");
        }

        var horario = await _horarioRepository.GetHorarioParaEstacionYFechaAsync(
            estacionId, fecha, estacion.UsaHorarioGenerico, cancellationToken);

        if (horario == null)
        {
            return ApiResponse<HorarioDto?>.Ok(null, "No hay horario configurado para esta fecha");
        }

        return ApiResponse<HorarioDto?>.Ok(MapToHorarioDto(horario));
    }

    public async Task<ApiResponse<bool>> EstaDisponibleAsync(
        int estacionId,
        DateTime fechaHora,
        CancellationToken cancellationToken = default)
    {
        var estacion = await _estacionRepository.GetByIdAsync(estacionId, cancellationToken);
        if (estacion == null)
        {
            return ApiResponse<bool>.Fail("Estación no encontrada");
        }

        if (!estacion.Activa || string.IsNullOrEmpty(estacion.BarberoId))
        {
            return ApiResponse<bool>.Ok(false, "La estación no está disponible para reservas");
        }

        // Obtener horario efectivo para la fecha
        var horario = await _horarioRepository.GetHorarioParaEstacionYFechaAsync(
            estacionId, fechaHora, estacion.UsaHorarioGenerico, cancellationToken);

        if (horario == null)
        {
            return ApiResponse<bool>.Ok(false, "No hay horario configurado para esta fecha");
        }

        // Verificar si es día bloqueado
        if (horario.Tipo == TipoHorario.Bloqueado)
        {
            return ApiResponse<bool>.Ok(false, "El día está bloqueado");
        }

        // Verificar si está dentro del horario
        var hora = fechaHora.TimeOfDay;

        if (hora < horario.HoraInicio || hora > horario.HoraFin)
        {
            return ApiResponse<bool>.Ok(false, "Fuera del horario de atención");
        }

        return ApiResponse<bool>.Ok(true, "Horario disponible");
    }

    public async Task<ApiResponse<HorarioDto>> BloquearDiaAsync(
        int estacionId,
        BloquearDiaRequest request,
        CancellationToken cancellationToken = default)
    {
        var estacion = await _estacionRepository.GetByIdAsync(estacionId, cancellationToken);
        if (estacion == null)
        {
            return ApiResponse<HorarioDto>.Fail("Estación no encontrada");
        }

        // Verificar si ya existe un bloqueo para esa fecha
        var existeBloqueo = await _context.ConfiguracionHorarios
            .AnyAsync(h => h.EstacionId == estacionId &&
                          h.Tipo == TipoHorario.Bloqueado &&
                          h.FechaVigenciaDesde.HasValue &&
                          h.FechaVigenciaDesde.Value.Date == request.Fecha.Date,
                      cancellationToken);

        if (existeBloqueo)
        {
            return ApiResponse<HorarioDto>.Fail("Ya existe un bloqueo para esa fecha");
        }

        var horario = new ConfiguracionHorario
        {
            EstacionId = estacionId,
            Tipo = TipoHorario.Bloqueado,
            DiaSemana = request.Fecha.DayOfWeek,
            HoraInicio = TimeSpan.Zero,
            HoraFin = TimeSpan.Zero,
            FechaVigenciaDesde = request.Fecha.Date,
            FechaVigenciaHasta = request.Fecha.Date,
            Descripcion = request.Motivo,
            Activo = true
        };

        await _horarioRepository.AddAsync(horario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Día bloqueado para estación {EstacionId}: {Fecha}",
            estacionId, request.Fecha.ToShortDateString());

        return ApiResponse<HorarioDto>.Ok(MapToHorarioDto(horario), "Día bloqueado correctamente");
    }

    public async Task<ApiResponse<HorarioDto>> GetHorarioByIdAsync(
        int horarioId,
        CancellationToken cancellationToken = default)
    {
        var horario = await _context.ConfiguracionHorarios
            .Include(h => h.Estacion)
            .FirstOrDefaultAsync(h => h.Id == horarioId, cancellationToken);

        if (horario == null)
        {
            return ApiResponse<HorarioDto>.Fail("Horario no encontrado");
        }

        return ApiResponse<HorarioDto>.Ok(MapToHorarioDto(horario));
    }

    #endregion

    #region Validaciones

    public async Task<bool> PuedeModificarHorariosAsync(
        int estacionId,
        string userId,
        bool esAdmin,
        CancellationToken cancellationToken = default)
    {
        if (esAdmin) return true;

        var estacion = await _estacionRepository.GetByIdAsync(estacionId, cancellationToken);
        return estacion?.BarberoId == userId;
    }

    #endregion

    #region Helpers

    private static HorarioSemanalDto BuildHorarioSemanalDto(
        int? estacionId,
        string? nombreEstacion,
        bool usaHorarioGenerico,
        List<ConfiguracionHorario> horarios)
    {
        var dto = new HorarioSemanalDto
        {
            EstacionId = estacionId,
            NombreEstacion = nombreEstacion,
            UsaHorarioGenerico = usaHorarioGenerico,
            Dias = new List<HorarioDiaDto>()
        };

        // Crear entrada para cada día de la semana
        for (int i = 0; i < 7; i++)
        {
            var diaSemana = (DayOfWeek)i;
            var horarioDia = horarios.FirstOrDefault(h => h.DiaSemana == diaSemana);

            dto.Dias.Add(new HorarioDiaDto
            {
                DiaSemana = diaSemana,
                Trabaja = horarioDia != null,
                HoraInicio = horarioDia?.HoraInicio,
                HoraFin = horarioDia?.HoraFin,
                HorarioId = horarioDia?.Id
            });
        }

        return dto;
    }

    private static HorarioDto MapToHorarioDto(ConfiguracionHorario horario)
    {
        return new HorarioDto
        {
            Id = horario.Id,
            EstacionId = horario.EstacionId,
            NombreEstacion = horario.Estacion?.Nombre,
            Tipo = horario.Tipo,
            DiaSemana = horario.DiaSemana,
            HoraInicio = horario.HoraInicio,
            HoraFin = horario.HoraFin,
            Activo = horario.Activo,
            FechaVigenciaDesde = horario.FechaVigenciaDesde,
            FechaVigenciaHasta = horario.FechaVigenciaHasta,
            Descripcion = horario.Descripcion
        };
    }

    #endregion
}


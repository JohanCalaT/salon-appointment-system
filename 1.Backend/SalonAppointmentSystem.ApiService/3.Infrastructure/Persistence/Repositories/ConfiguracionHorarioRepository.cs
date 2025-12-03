using Microsoft.EntityFrameworkCore;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio específico para ConfiguracionHorario
/// </summary>
public class ConfiguracionHorarioRepository : Repository<ConfiguracionHorario>, IConfiguracionHorarioRepository
{
    public ConfiguracionHorarioRepository(ApplicationDbContext context) : base(context)
    {
    }

    #region Horarios Globales (EstacionId = null)

    public async Task<IReadOnlyList<ConfiguracionHorario>> GetGlobalesByDiaSemanaAsync(
        DayOfWeek diaSemana,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.EstacionId == null &&
                        c.DiaSemana == diaSemana &&
                        c.Activo &&
                        c.Tipo == TipoHorario.Regular)
            .OrderBy(c => c.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConfiguracionHorario>> GetHorariosGlobalesSemanaAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.EstacionId == null &&
                        c.Activo &&
                        c.Tipo == TipoHorario.Regular)
            .OrderBy(c => c.DiaSemana)
            .ThenBy(c => c.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<ConfiguracionHorario?> GetHorarioGlobalParaFechaAsync(
        DateTime fecha,
        CancellationToken cancellationToken = default)
    {
        var diaSemana = fecha.DayOfWeek;
        var fechaSolo = fecha.Date;

        // Primero buscar horarios especiales globales con vigencia
        var horarioEspecial = await _dbSet
            .Where(c => c.EstacionId == null &&
                        c.Activo &&
                        c.Tipo == TipoHorario.Especial &&
                        c.FechaVigenciaDesde.HasValue &&
                        c.FechaVigenciaDesde.Value.Date <= fechaSolo &&
                        (!c.FechaVigenciaHasta.HasValue || c.FechaVigenciaHasta.Value.Date >= fechaSolo))
            .FirstOrDefaultAsync(cancellationToken);

        if (horarioEspecial != null)
            return horarioEspecial;

        // Si no hay especial, buscar horario regular global
        return await _dbSet
            .Where(c => c.EstacionId == null &&
                        c.DiaSemana == diaSemana &&
                        c.Activo &&
                        c.Tipo == TipoHorario.Regular)
            .FirstOrDefaultAsync(cancellationToken);
    }

    #endregion

    #region Horarios por Estación

    public async Task<IReadOnlyList<ConfiguracionHorario>> GetByEstacionYDiaSemanaAsync(
        int estacionId,
        DayOfWeek diaSemana,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.EstacionId == estacionId &&
                        c.DiaSemana == diaSemana &&
                        c.Activo &&
                        c.Tipo == TipoHorario.Regular)
            .OrderBy(c => c.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConfiguracionHorario>> GetHorariosSemanaByEstacionAsync(
        int estacionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.EstacionId == estacionId &&
                        c.Activo &&
                        c.Tipo == TipoHorario.Regular)
            .OrderBy(c => c.DiaSemana)
            .ThenBy(c => c.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<ConfiguracionHorario?> GetHorarioParaEstacionYFechaAsync(
        int estacionId,
        DateTime fecha,
        bool usaHorarioGenerico,
        CancellationToken cancellationToken = default)
    {
        var diaSemana = fecha.DayOfWeek;
        var fechaSolo = fecha.Date;

        // 1. Verificar si hay un día bloqueado para esta estación
        var diaBloqueado = await _dbSet
            .Where(c => c.EstacionId == estacionId &&
                        c.Activo &&
                        c.Tipo == TipoHorario.Bloqueado &&
                        c.FechaVigenciaDesde.HasValue &&
                        c.FechaVigenciaDesde.Value.Date <= fechaSolo &&
                        (!c.FechaVigenciaHasta.HasValue || c.FechaVigenciaHasta.Value.Date >= fechaSolo))
            .FirstOrDefaultAsync(cancellationToken);

        if (diaBloqueado != null)
            return diaBloqueado;

        // 2. Buscar horario especial de la estación para esa fecha
        var horarioEspecial = await _dbSet
            .Where(c => c.EstacionId == estacionId &&
                        c.Activo &&
                        c.Tipo == TipoHorario.Especial &&
                        c.FechaVigenciaDesde.HasValue &&
                        c.FechaVigenciaDesde.Value.Date <= fechaSolo &&
                        (!c.FechaVigenciaHasta.HasValue || c.FechaVigenciaHasta.Value.Date >= fechaSolo))
            .FirstOrDefaultAsync(cancellationToken);

        if (horarioEspecial != null)
            return horarioEspecial;

        // 3. Si usa horario genérico, buscar el horario global
        if (usaHorarioGenerico)
        {
            return await GetHorarioGlobalParaFechaAsync(fecha, cancellationToken);
        }

        // 4. Buscar horario regular personalizado de la estación
        return await _dbSet
            .Where(c => c.EstacionId == estacionId &&
                        c.DiaSemana == diaSemana &&
                        c.Activo &&
                        c.Tipo == TipoHorario.Regular)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConfiguracionHorario>> GetHorariosEspecialesByEstacionAsync(
        int estacionId,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(c => c.EstacionId == estacionId &&
                        c.Activo &&
                        c.Tipo == TipoHorario.Especial);

        if (desde.HasValue)
        {
            query = query.Where(c => !c.FechaVigenciaHasta.HasValue ||
                                     c.FechaVigenciaHasta.Value.Date >= desde.Value.Date);
        }

        if (hasta.HasValue)
        {
            query = query.Where(c => c.FechaVigenciaDesde.HasValue &&
                                     c.FechaVigenciaDesde.Value.Date <= hasta.Value.Date);
        }

        return await query
            .OrderBy(c => c.FechaVigenciaDesde)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConfiguracionHorario>> GetDiasBloqueadosByEstacionAsync(
        int estacionId,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.EstacionId == estacionId &&
                        c.Activo &&
                        c.Tipo == TipoHorario.Bloqueado &&
                        c.FechaVigenciaDesde.HasValue &&
                        c.FechaVigenciaDesde.Value.Date <= hasta.Date &&
                        (!c.FechaVigenciaHasta.HasValue || c.FechaVigenciaHasta.Value.Date >= desde.Date))
            .OrderBy(c => c.FechaVigenciaDesde)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Validaciones

    public async Task<bool> ExisteSolapamientoAsync(
        int? estacionId,
        DayOfWeek diaSemana,
        TimeSpan horaInicio,
        TimeSpan horaFin,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(c => c.EstacionId == estacionId &&
                        c.DiaSemana == diaSemana &&
                        c.Activo &&
                        c.Tipo == TipoHorario.Regular &&
                        c.HoraInicio < horaFin &&
                        c.HoraFin > horaInicio);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExisteHorarioEspecialEnFechasAsync(
        int estacionId,
        DateTime fechaDesde,
        DateTime? fechaHasta,
        TipoHorario tipo,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var fechaHastaReal = fechaHasta ?? fechaDesde;

        var query = _dbSet
            .Where(c => c.EstacionId == estacionId &&
                        c.Activo &&
                        c.Tipo == tipo &&
                        c.FechaVigenciaDesde.HasValue &&
                        c.FechaVigenciaDesde.Value.Date <= fechaHastaReal.Date &&
                        (!c.FechaVigenciaHasta.HasValue || c.FechaVigenciaHasta.Value.Date >= fechaDesde.Date));

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    #endregion

    #region Compatibilidad (métodos anteriores)

    public async Task<IReadOnlyList<ConfiguracionHorario>> GetByDiaSemanaAsync(
        DayOfWeek diaSemana,
        CancellationToken cancellationToken = default)
    {
        // Mantiene compatibilidad: devuelve horarios globales
        return await GetGlobalesByDiaSemanaAsync(diaSemana, cancellationToken);
    }

    public async Task<ConfiguracionHorario?> GetHorarioParaFechaAsync(
        DateTime fecha,
        CancellationToken cancellationToken = default)
    {
        // Mantiene compatibilidad: devuelve horario global
        return await GetHorarioGlobalParaFechaAsync(fecha, cancellationToken);
    }

    public async Task<IReadOnlyList<ConfiguracionHorario>> GetHorariosSemanaAsync(
        CancellationToken cancellationToken = default)
    {
        // Mantiene compatibilidad: devuelve horarios globales
        return await GetHorariosGlobalesSemanaAsync(cancellationToken);
    }

    #endregion
}


using Microsoft.EntityFrameworkCore;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio específico para ConfiguracionHorario
/// </summary>
public class ConfiguracionHorarioRepository : Repository<ConfiguracionHorario>, IConfiguracionHorarioRepository
{
    public ConfiguracionHorarioRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ConfiguracionHorario>> GetByDiaSemanaAsync(
        DayOfWeek diaSemana,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.DiaSemana == diaSemana && c.Activo)
            .OrderBy(c => c.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<ConfiguracionHorario?> GetHorarioParaFechaAsync(
        DateTime fecha,
        CancellationToken cancellationToken = default)
    {
        var diaSemana = fecha.DayOfWeek;
        var fechaSolo = fecha.Date;

        // Primero buscar horarios especiales con vigencia
        var horarioEspecial = await _dbSet
            .Where(c => c.DiaSemana == diaSemana &&
                        c.Activo &&
                        c.FechaVigenciaDesde.HasValue &&
                        c.FechaVigenciaDesde.Value.Date <= fechaSolo &&
                        (!c.FechaVigenciaHasta.HasValue || c.FechaVigenciaHasta.Value.Date >= fechaSolo))
            .FirstOrDefaultAsync(cancellationToken);

        if (horarioEspecial != null)
            return horarioEspecial;

        // Si no hay especial, buscar horario regular (sin fechas de vigencia)
        return await _dbSet
            .Where(c => c.DiaSemana == diaSemana &&
                        c.Activo &&
                        !c.FechaVigenciaDesde.HasValue &&
                        !c.FechaVigenciaHasta.HasValue)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConfiguracionHorario>> GetHorariosSemanaAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Activo &&
                        !c.FechaVigenciaDesde.HasValue &&
                        !c.FechaVigenciaHasta.HasValue)
            .OrderBy(c => c.DiaSemana)
            .ThenBy(c => c.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteSolapamientoAsync(
        DayOfWeek diaSemana,
        TimeSpan horaInicio,
        TimeSpan horaFin,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(c => c.DiaSemana == diaSemana &&
                        c.Activo &&
                        c.HoraInicio < horaFin &&
                        c.HoraFin > horaInicio);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}


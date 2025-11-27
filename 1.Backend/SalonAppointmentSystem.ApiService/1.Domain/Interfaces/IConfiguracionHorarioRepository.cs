using SalonAppointmentSystem.ApiService.Domain.Entities;

namespace SalonAppointmentSystem.ApiService.Domain.Interfaces;

/// <summary>
/// Repositorio específico para ConfiguracionHorario
/// </summary>
public interface IConfiguracionHorarioRepository : IRepository<ConfiguracionHorario>
{
    /// <summary>
    /// Obtiene los horarios activos para un día de la semana
    /// </summary>
    Task<IReadOnlyList<ConfiguracionHorario>> GetByDiaSemanaAsync(
        DayOfWeek diaSemana,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el horario aplicable para una fecha específica
    /// </summary>
    Task<ConfiguracionHorario?> GetHorarioParaFechaAsync(
        DateTime fecha,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los horarios de la semana
    /// </summary>
    Task<IReadOnlyList<ConfiguracionHorario>> GetHorariosSemanaAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si hay solapamiento con otro horario para el mismo día
    /// </summary>
    Task<bool> ExisteSolapamientoAsync(
        DayOfWeek diaSemana,
        TimeSpan horaInicio,
        TimeSpan horaFin,
        int? excludeId = null,
        CancellationToken cancellationToken = default);
}


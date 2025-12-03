using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Domain.Interfaces;

/// <summary>
/// Repositorio específico para ConfiguracionHorario
/// </summary>
public interface IConfiguracionHorarioRepository : IRepository<ConfiguracionHorario>
{
    #region Horarios Globales (EstacionId = null)

    /// <summary>
    /// Obtiene los horarios globales activos para un día de la semana
    /// </summary>
    Task<IReadOnlyList<ConfiguracionHorario>> GetGlobalesByDiaSemanaAsync(
        DayOfWeek diaSemana,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los horarios globales de la semana (regulares)
    /// </summary>
    Task<IReadOnlyList<ConfiguracionHorario>> GetHorariosGlobalesSemanaAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el horario global aplicable para una fecha específica
    /// </summary>
    Task<ConfiguracionHorario?> GetHorarioGlobalParaFechaAsync(
        DateTime fecha,
        CancellationToken cancellationToken = default);

    #endregion

    #region Horarios por Estación

    /// <summary>
    /// Obtiene los horarios de una estación para un día de la semana
    /// </summary>
    Task<IReadOnlyList<ConfiguracionHorario>> GetByEstacionYDiaSemanaAsync(
        int estacionId,
        DayOfWeek diaSemana,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los horarios semanales de una estación
    /// </summary>
    Task<IReadOnlyList<ConfiguracionHorario>> GetHorariosSemanaByEstacionAsync(
        int estacionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el horario aplicable para una estación en una fecha específica
    /// Considera la jerarquía: Especial > Personalizado > Global
    /// </summary>
    Task<ConfiguracionHorario?> GetHorarioParaEstacionYFechaAsync(
        int estacionId,
        DateTime fecha,
        bool usaHorarioGenerico,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los horarios especiales de una estación
    /// </summary>
    Task<IReadOnlyList<ConfiguracionHorario>> GetHorariosEspecialesByEstacionAsync(
        int estacionId,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los días bloqueados de una estación en un rango de fechas
    /// </summary>
    Task<IReadOnlyList<ConfiguracionHorario>> GetDiasBloqueadosByEstacionAsync(
        int estacionId,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);

    #endregion

    #region Validaciones

    /// <summary>
    /// Verifica si hay solapamiento con otro horario para el mismo día y estación
    /// </summary>
    Task<bool> ExisteSolapamientoAsync(
        int? estacionId,
        DayOfWeek diaSemana,
        TimeSpan horaInicio,
        TimeSpan horaFin,
        int? excludeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si ya existe un horario especial o bloqueado para las fechas indicadas
    /// </summary>
    Task<bool> ExisteHorarioEspecialEnFechasAsync(
        int estacionId,
        DateTime fechaDesde,
        DateTime? fechaHasta,
        TipoHorario tipo,
        int? excludeId = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Compatibilidad (mantener métodos anteriores)

    /// <summary>
    /// Obtiene los horarios activos para un día de la semana (global + estaciones)
    /// </summary>
    Task<IReadOnlyList<ConfiguracionHorario>> GetByDiaSemanaAsync(
        DayOfWeek diaSemana,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el horario aplicable para una fecha específica (solo global)
    /// </summary>
    Task<ConfiguracionHorario?> GetHorarioParaFechaAsync(
        DateTime fecha,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los horarios globales de la semana
    /// </summary>
    Task<IReadOnlyList<ConfiguracionHorario>> GetHorariosSemanaAsync(
        CancellationToken cancellationToken = default);

    #endregion
}


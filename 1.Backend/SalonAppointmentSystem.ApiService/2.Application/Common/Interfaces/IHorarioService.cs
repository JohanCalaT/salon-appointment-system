using SalonAppointmentSystem.Shared.DTOs.Horarios;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Application.Common.Interfaces;

/// <summary>
/// Interface para el servicio de gestión de horarios
/// </summary>
public interface IHorarioService
{
    #region Horarios Globales (Solo Admin)

    /// <summary>
    /// Obtiene el horario semanal global del negocio
    /// </summary>
    Task<ApiResponse<HorarioSemanalDto>> GetHorarioGlobalAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Configura el horario semanal global del negocio
    /// </summary>
    Task<ApiResponse<HorarioSemanalDto>> ConfigurarHorarioGlobalAsync(
        ConfigurarHorarioSemanalRequest request,
        CancellationToken cancellationToken = default);

    #endregion

    #region Horarios por Estación (Admin o Barbero propietario)

    /// <summary>
    /// Obtiene el horario semanal de una estación
    /// </summary>
    Task<ApiResponse<HorarioSemanalDto>> GetHorarioEstacionAsync(
        int estacionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Configura el horario semanal de una estación
    /// </summary>
    Task<ApiResponse<HorarioSemanalDto>> ConfigurarHorarioEstacionAsync(
        int estacionId,
        ConfigurarHorarioSemanalRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cambia entre horario genérico y personalizado
    /// </summary>
    Task<ApiResponse<HorarioSemanalDto>> CambiarTipoHorarioAsync(
        int estacionId,
        CambiarTipoHorarioRequest request,
        CancellationToken cancellationToken = default);

    #endregion

    #region Horarios Especiales y Días Bloqueados

    /// <summary>
    /// Crea un horario especial o día bloqueado para una estación
    /// </summary>
    Task<ApiResponse<HorarioDto>> CrearHorarioEspecialAsync(
        int estacionId,
        HorarioEspecialRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un horario especial o día bloqueado
    /// </summary>
    Task<ApiResponse<HorarioDto>> ActualizarHorarioEspecialAsync(
        int horarioId,
        UpdateHorarioEspecialRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un horario especial o día bloqueado
    /// </summary>
    Task<ApiResponse<bool>> EliminarHorarioEspecialAsync(
        int horarioId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los horarios especiales de una estación en un rango de fechas
    /// </summary>
    Task<ApiResponse<IReadOnlyList<HorarioDto>>> GetHorariosEspecialesAsync(
        int estacionId,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los días bloqueados de una estación en un rango de fechas
    /// </summary>
    Task<ApiResponse<IReadOnlyList<HorarioDto>>> GetDiasBloqueadosAsync(
        int estacionId,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bloquea un día completo para una estación
    /// </summary>
    Task<ApiResponse<HorarioDto>> BloquearDiaAsync(
        int estacionId,
        BloquearDiaRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un horario por su ID
    /// </summary>
    Task<ApiResponse<HorarioDto>> GetHorarioByIdAsync(
        int horarioId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Consultas de Disponibilidad

    /// <summary>
    /// Obtiene el horario efectivo para una estación en una fecha específica
    /// (considera horarios especiales, bloqueados, genéricos, etc.)
    /// </summary>
    Task<ApiResponse<HorarioDto?>> GetHorarioEfectivoAsync(
        int estacionId,
        DateTime fecha,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si una estación está disponible en una fecha y hora específica
    /// </summary>
    Task<ApiResponse<bool>> EstaDisponibleAsync(
        int estacionId,
        DateTime fechaHora,
        CancellationToken cancellationToken = default);

    #endregion

    #region Validaciones

    /// <summary>
    /// Verifica si el usuario actual puede modificar los horarios de una estación
    /// </summary>
    Task<bool> PuedeModificarHorariosAsync(
        int estacionId,
        string userId,
        bool esAdmin,
        CancellationToken cancellationToken = default);

    #endregion
}


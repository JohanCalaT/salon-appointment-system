using SalonAppointmentSystem.Shared.DTOs.Estaciones;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Application.Common.Interfaces;

/// <summary>
/// Interface para el servicio de gestión de estaciones
/// </summary>
public interface IEstacionService
{
    #region Lectura (Admin y Barbero)

    /// <summary>
    /// Obtiene una estación por su ID
    /// </summary>
    Task<ApiResponse<EstacionDto>> GetByIdAsync(int estacionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las estaciones sin paginación
    /// </summary>
    Task<ApiResponse<IReadOnlyList<EstacionDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estaciones con paginación y filtros
    /// </summary>
    Task<ApiResponse<PagedResult<EstacionDto>>> GetPagedAsync(EstacionListFilters filters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la estación asignada a un barbero
    /// </summary>
    Task<ApiResponse<EstacionDto>> GetByBarberoIdAsync(string barberoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene solo las estaciones activas que pueden recibir reservas
    /// </summary>
    Task<ApiResponse<IReadOnlyList<EstacionDto>>> GetActivasParaReservasAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Escritura (Solo Admin)

    /// <summary>
    /// Crea una nueva estación
    /// </summary>
    Task<ApiResponse<EstacionDto>> CreateAsync(CreateEstacionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza completamente una estación (PUT)
    /// </summary>
    Task<ApiResponse<EstacionDto>> UpdateAsync(int estacionId, UpdateEstacionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una estación (soft delete - desactiva)
    /// </summary>
    Task<ApiResponse<bool>> DeleteAsync(int estacionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activa o desactiva una estación
    /// </summary>
    Task<ApiResponse<bool>> ToggleActiveAsync(int estacionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asigna o desasigna un barbero a una estación
    /// </summary>
    Task<ApiResponse<EstacionDto>> AsignarBarberoAsync(int estacionId, AsignarBarberoRequest request, CancellationToken cancellationToken = default);

    #endregion

    #region Validaciones

    /// <summary>
    /// Verifica si un nombre de estación ya existe
    /// </summary>
    Task<bool> NombreExistsAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un barbero ya está asignado a otra estación
    /// </summary>
    Task<bool> BarberoYaAsignadoAsync(string barberoId, int? excludeEstacionId = null, CancellationToken cancellationToken = default);

    #endregion
}


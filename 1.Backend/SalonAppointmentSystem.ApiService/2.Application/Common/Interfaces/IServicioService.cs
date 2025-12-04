using SalonAppointmentSystem.Shared.DTOs.Servicios;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Application.Common.Interfaces;

/// <summary>
/// Interface para el servicio de gestión de servicios de barbería
/// </summary>
public interface IServicioService
{
    #region Lectura (Admin y Barbero)

    /// <summary>
    /// Obtiene un servicio por su ID
    /// </summary>
    Task<ApiResponse<ServicioDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los servicios sin paginación
    /// </summary>
    Task<ApiResponse<IReadOnlyList<ServicioDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene servicios con paginación y filtros
    /// </summary>
    Task<ApiResponse<PagedResult<ServicioDto>>> GetPagedAsync(ServicioListFilters filters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene solo los servicios activos (para reservas)
    /// </summary>
    Task<ApiResponse<IReadOnlyList<ServicioDto>>> GetActivosAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Escritura (Admin y Barbero)

    /// <summary>
    /// Crea un nuevo servicio
    /// </summary>
    Task<ApiResponse<ServicioDto>> CreateAsync(CreateServicioRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza completamente un servicio (PUT)
    /// </summary>
    Task<ApiResponse<ServicioDto>> UpdateAsync(int id, UpdateServicioRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza parcialmente un servicio (PATCH)
    /// </summary>
    Task<ApiResponse<ServicioDto>> PatchAsync(int id, PatchServicioRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un servicio (soft delete - desactiva)
    /// </summary>
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activa o desactiva un servicio
    /// </summary>
    Task<ApiResponse<bool>> ToggleActivoAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Validaciones

    /// <summary>
    /// Verifica si un nombre de servicio ya existe
    /// </summary>
    Task<bool> NombreExistsAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default);

    #endregion
}


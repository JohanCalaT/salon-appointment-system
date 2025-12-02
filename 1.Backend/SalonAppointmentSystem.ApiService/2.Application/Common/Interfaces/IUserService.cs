using SalonAppointmentSystem.Shared.DTOs.Users;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Application.Common.Interfaces;

/// <summary>
/// Interface para el servicio de gestión de usuarios
/// </summary>
public interface IUserService
{
    #region Lectura (Admin y Barbero)

    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Usuario encontrado o error</returns>
    Task<ApiResponse<UserDto>> GetByIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los usuarios sin paginación
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de todos los usuarios</returns>
    Task<ApiResponse<IReadOnlyList<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene usuarios con paginación y filtros
    /// </summary>
    /// <param name="filters">Filtros de búsqueda y paginación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado paginado de usuarios</returns>
    Task<ApiResponse<PagedResult<UserDto>>> GetPagedAsync(UserListFilters filters, CancellationToken cancellationToken = default);

    #endregion

    #region Escritura (Solo Admin)

    /// <summary>
    /// Crea un nuevo usuario con rol específico
    /// </summary>
    /// <param name="request">Datos del nuevo usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Usuario creado</returns>
    Task<ApiResponse<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza completamente un usuario (PUT)
    /// </summary>
    /// <param name="userId">ID del usuario a actualizar</param>
    /// <param name="request">Nuevos datos del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Usuario actualizado</returns>
    Task<ApiResponse<UserDto>> UpdateAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza parcialmente un usuario (PATCH)
    /// Solo se actualizan los campos con valor
    /// </summary>
    /// <param name="userId">ID del usuario a actualizar</param>
    /// <param name="request">Campos a actualizar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Usuario actualizado</returns>
    Task<ApiResponse<UserDto>> PatchAsync(string userId, PatchUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un usuario (soft delete - desactiva)
    /// </summary>
    /// <param name="userId">ID del usuario a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se eliminó correctamente</returns>
    Task<ApiResponse<bool>> DeleteAsync(string userId, CancellationToken cancellationToken = default);

    #endregion

    #region Acciones Especiales (Solo Admin)

    /// <summary>
    /// Cambia la contraseña de un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="newPassword">Nueva contraseña</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se cambió correctamente</returns>
    Task<ApiResponse<bool>> ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activa o desactiva un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Nuevo estado del usuario</returns>
    Task<ApiResponse<bool>> ToggleActiveAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un email ya está registrado
    /// </summary>
    /// <param name="email">Email a verificar</param>
    /// <param name="excludeUserId">ID de usuario a excluir (para updates)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si el email ya existe</returns>
    Task<bool> EmailExistsAsync(string email, string? excludeUserId = null, CancellationToken cancellationToken = default);

    #endregion
}


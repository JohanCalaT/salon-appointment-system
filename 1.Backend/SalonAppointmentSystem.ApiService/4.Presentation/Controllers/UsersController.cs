using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonAppointmentSystem.ApiService.Application.Common.Constants;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.Shared.DTOs.Users;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Presentation.Controllers;

/// <summary>
/// Controlador para gestión de usuarios
/// Admin: Acceso completo (CRUD)
/// Barbero: Solo lectura
/// Cliente/Invitado: Sin acceso
/// </summary>
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    #region Lectura (Admin y Barbero)

    /// <summary>
    /// Obtiene la lista de usuarios paginada con filtros
    /// </summary>
    /// <param name="filters">Filtros de búsqueda y paginación</param>
    /// <returns>Lista paginada de usuarios</returns>
    [HttpGet]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPaged([FromQuery] UserListFilters filters)
    {
        var result = await _userService.GetPagedAsync(filters);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene la lista completa de usuarios sin paginación
    /// </summary>
    /// <returns>Lista completa de usuarios</returns>
    [HttpGet("all")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Datos del usuario</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _userService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    #endregion

    #region Escritura (Solo Admin)

    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    /// <param name="request">Datos del nuevo usuario</param>
    /// <returns>Usuario creado</returns>
    [HttpPost]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var result = await _userService.CreateAsync(request);

        if (!result.Success)
        {
            // Email duplicado -> 409 Conflict
            if (result.Message.Contains("ya está registrado"))
            {
                return Conflict(result);
            }
            return BadRequest(result);
        }

        _logger.LogInformation("Usuario creado por {AdminId}: {Email}", CurrentUserId, request.Email);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Actualiza completamente un usuario (PUT)
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="request">Nuevos datos del usuario</param>
    /// <returns>Usuario actualizado</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateAsync(id, request);

        if (!result.Success)
        {
            if (result.Message.Contains("no encontrado"))
            {
                return NotFound(result);
            }
            if (result.Message.Contains("ya está registrado"))
            {
                return Conflict(result);
            }
            return BadRequest(result);
        }

        _logger.LogInformation("Usuario actualizado por {AdminId}: {UserId}", CurrentUserId, id);
        return Ok(result);
    }

    /// <summary>
    /// Actualiza parcialmente un usuario (PATCH)
    /// Solo se actualizan los campos enviados
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="request">Campos a actualizar</param>
    /// <returns>Usuario actualizado</returns>
    [HttpPatch("{id}")]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Patch(string id, [FromBody] PatchUserRequest request)
    {
        var result = await _userService.PatchAsync(id, request);

        if (!result.Success)
        {
            if (result.Message.Contains("no encontrado"))
            {
                return NotFound(result);
            }
            if (result.Message.Contains("ya está registrado"))
            {
                return Conflict(result);
            }
            return BadRequest(result);
        }

        _logger.LogInformation("Usuario actualizado parcialmente por {AdminId}: {UserId}", CurrentUserId, id);
        return Ok(result);
    }

    /// <summary>
    /// Elimina un usuario (soft delete)
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(string id)
    {
        // Evitar que el admin se elimine a sí mismo
        if (id == CurrentUserId)
        {
            return BadRequest(ApiResponse<bool>.Fail("No puedes eliminarte a ti mismo"));
        }

        var result = await _userService.DeleteAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        _logger.LogInformation("Usuario eliminado por {AdminId}: {UserId}", CurrentUserId, id);
        return NoContent();
    }

    #endregion

    #region Acciones Especiales (Solo Admin)

    /// <summary>
    /// Activa o desactiva un usuario
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Nuevo estado del usuario</returns>
    [HttpPost("{id}/toggle-active")]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ToggleActive(string id)
    {
        // Evitar que el admin se desactive a sí mismo
        if (id == CurrentUserId)
        {
            return BadRequest(ApiResponse<bool>.Fail("No puedes desactivarte a ti mismo"));
        }

        var result = await _userService.ToggleActiveAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        _logger.LogInformation("Usuario {Estado} por {AdminId}: {UserId}",
            result.Data ? "activado" : "desactivado", CurrentUserId, id);
        return Ok(result);
    }

    /// <summary>
    /// Resetea la contraseña de un usuario
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="request">Nueva contraseña</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPost("{id}/reset-password")]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordRequest request)
    {
        var result = await _userService.ResetPasswordAsync(id, request.NewPassword);

        if (!result.Success)
        {
            if (result.Message.Contains("no encontrado"))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        _logger.LogInformation("Contraseña reseteada por {AdminId} para usuario: {UserId}", CurrentUserId, id);
        return Ok(result);
    }

    #endregion
}


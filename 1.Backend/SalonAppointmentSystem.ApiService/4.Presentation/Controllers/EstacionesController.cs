using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonAppointmentSystem.ApiService.Application.Common.Constants;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.Shared.DTOs.Estaciones;
using SalonAppointmentSystem.Shared.Models;
using System.Security.Claims;

namespace SalonAppointmentSystem.ApiService.Presentation.Controllers;

/// <summary>
/// Controlador para gestión de estaciones
/// Admin: Acceso completo (CRUD)
/// Barbero: Solo lectura de su estación
/// Cliente/Invitado: Sin acceso
/// </summary>
public class EstacionesController : ApiControllerBase
{
    private readonly IEstacionService _estacionService;
    private readonly ILogger<EstacionesController> _logger;

    public EstacionesController(IEstacionService estacionService, ILogger<EstacionesController> logger)
    {
        _estacionService = estacionService;
        _logger = logger;
    }

    #region Lectura (Admin y Barbero)

    /// <summary>
    /// Obtiene la lista de estaciones paginada con filtros
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EstacionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] EstacionListFilters filters)
    {
        // Si es barbero, solo puede ver su estación
        if (!User.IsInRole(ApplicationRoles.Admin))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            filters.BarberoId = userId;
        }

        var result = await _estacionService.GetPagedAsync(filters);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene todas las estaciones sin paginación
    /// </summary>
    [HttpGet("all")]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EstacionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _estacionService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene una estación por su ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<EstacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EstacionDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _estacionService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        // Si es barbero, verificar que sea su estación
        if (!User.IsInRole(ApplicationRoles.Admin))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (result.Data?.BarberoId != userId)
            {
                return Forbid();
            }
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtiene la estación del barbero autenticado
    /// </summary>
    [HttpGet("mi-estacion")]
    [Authorize(Roles = ApplicationRoles.Barbero)]
    [ProducesResponseType(typeof(ApiResponse<EstacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EstacionDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMiEstacion()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _estacionService.GetByBarberoIdAsync(userId!);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtiene las estaciones activas disponibles para reservas
    /// </summary>
    [HttpGet("activas")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EstacionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivas()
    {
        var result = await _estacionService.GetActivasParaReservasAsync();
        return Ok(result);
    }

    #endregion

    #region Escritura (Solo Admin)

    /// <summary>
    /// Crea una nueva estación
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<EstacionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<EstacionDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEstacionRequest request)
    {
        var result = await _estacionService.CreateAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Estación creada por {UserId}: {Nombre}",
            User.FindFirstValue(ClaimTypes.NameIdentifier), result.Data?.Nombre);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Actualiza una estación existente
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<EstacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EstacionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EstacionDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEstacionRequest request)
    {
        var result = await _estacionService.UpdateAsync(id, request);

        if (!result.Success)
        {
            if (result.Message.Contains("no encontrada"))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        _logger.LogInformation("Estación actualizada por {UserId}: {Id}",
            User.FindFirstValue(ClaimTypes.NameIdentifier), id);

        return Ok(result);
    }

    /// <summary>
    /// Elimina una estación (soft delete)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _estacionService.DeleteAsync(id);

        if (!result.Success)
        {
            if (result.Message.Contains("no encontrada"))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        _logger.LogInformation("Estación eliminada por {UserId}: {Id}",
            User.FindFirstValue(ClaimTypes.NameIdentifier), id);

        return Ok(result);
    }

    /// <summary>
    /// Activa o desactiva una estación
    /// </summary>
    [HttpPost("{id:int}/toggle-active")]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _estacionService.ToggleActiveAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        _logger.LogInformation("Estación {Estado} por {UserId}: {Id}",
            result.Data ? "activada" : "desactivada",
            User.FindFirstValue(ClaimTypes.NameIdentifier), id);

        return Ok(result);
    }

    /// <summary>
    /// Asigna o desasigna un barbero a una estación
    /// </summary>
    [HttpPost("{id:int}/asignar-barbero")]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<EstacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EstacionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EstacionDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AsignarBarbero(int id, [FromBody] AsignarBarberoRequest request)
    {
        var result = await _estacionService.AsignarBarberoAsync(id, request);

        if (!result.Success)
        {
            if (result.Message.Contains("no encontrada"))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        _logger.LogInformation("Barbero {Accion} en estación {Id} por {UserId}",
            string.IsNullOrEmpty(request.BarberoId) ? "desasignado" : "asignado",
            id, User.FindFirstValue(ClaimTypes.NameIdentifier));

        return Ok(result);
    }

    #endregion
}


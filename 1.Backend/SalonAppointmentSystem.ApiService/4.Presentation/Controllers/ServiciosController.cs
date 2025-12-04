using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonAppointmentSystem.ApiService.Application.Common.Constants;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.Shared.DTOs.Servicios;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Presentation.Controllers;

/// <summary>
/// Controlador para gestión de servicios de barbería
/// Admin y Barbero: Acceso completo (CRUD)
/// Cliente/Invitado: Sin acceso
/// </summary>
public class ServiciosController : ApiControllerBase
{
    private readonly IServicioService _servicioService;
    private readonly ILogger<ServiciosController> _logger;

    public ServiciosController(IServicioService servicioService, ILogger<ServiciosController> logger)
    {
        _servicioService = servicioService;
        _logger = logger;
    }

    #region Lectura (Admin y Barbero)

    /// <summary>
    /// Obtiene la lista de servicios paginada con filtros
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ServicioDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] ServicioListFilters filters)
    {
        var result = await _servicioService.GetPagedAsync(filters);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene todos los servicios sin paginación
    /// </summary>
    [HttpGet("all")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ServicioDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _servicioService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene solo los servicios activos (para reservas)
    /// </summary>
    [HttpGet("activos")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ServicioDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivos()
    {
        var result = await _servicioService.GetActivosAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un servicio por su ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<ServicioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ServicioDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _servicioService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    #endregion

    #region Escritura (Admin y Barbero)

    /// <summary>
    /// Crea un nuevo servicio
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<ServicioDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ServicioDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateServicioRequest request)
    {
        var result = await _servicioService.CreateAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Actualiza completamente un servicio (PUT)
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<ServicioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ServicioDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ServicioDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServicioRequest request)
    {
        var result = await _servicioService.UpdateAsync(id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("no encontrado") == true)
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Actualiza parcialmente un servicio (PATCH)
    /// </summary>
    [HttpPatch("{id:int}")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<ServicioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ServicioDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ServicioDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchServicioRequest request)
    {
        var result = await _servicioService.PatchAsync(id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("no encontrado") == true)
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Elimina un servicio (soft delete - desactiva)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _servicioService.DeleteAsync(id);

        if (!result.Success)
        {
            if (result.Message?.Contains("no encontrado") == true)
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Activa o desactiva un servicio
    /// </summary>
    [HttpPost("{id:int}/toggle-activo")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleActivo(int id)
    {
        var result = await _servicioService.ToggleActivoAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    #endregion
}

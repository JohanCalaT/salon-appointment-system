using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonAppointmentSystem.ApiService.Application.Common.Constants;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.Shared.DTOs.Horarios;
using SalonAppointmentSystem.Shared.Models;
using System.Security.Claims;

namespace SalonAppointmentSystem.ApiService.Presentation.Controllers;

/// <summary>
/// Controlador para gestión de horarios
/// Admin: Gestiona horarios globales y de todas las estaciones
/// Barbero: Solo puede gestionar horarios de su estación
/// </summary>
public class HorariosController : ApiControllerBase
{
    private readonly IHorarioService _horarioService;
    private readonly IEstacionService _estacionService;
    private readonly ILogger<HorariosController> _logger;

    public HorariosController(
        IHorarioService horarioService,
        IEstacionService estacionService,
        ILogger<HorariosController> logger)
    {
        _horarioService = horarioService;
        _estacionService = estacionService;
        _logger = logger;
    }

    #region Horarios Globales (Solo Admin)

    /// <summary>
    /// Obtiene el horario semanal global del negocio
    /// </summary>
    [HttpGet("global")]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<HorarioSemanalDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHorarioGlobal()
    {
        var result = await _horarioService.GetHorarioGlobalAsync();
        return Ok(result);
    }

    /// <summary>
    /// Configura el horario semanal global del negocio
    /// </summary>
    [HttpPut("global")]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<HorarioSemanalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HorarioSemanalDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfigurarHorarioGlobal([FromBody] ConfigurarHorarioSemanalRequest request)
    {
        var result = await _horarioService.ConfigurarHorarioGlobalAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Horario global configurado por {UserId}",
            User.FindFirstValue(ClaimTypes.NameIdentifier));

        return Ok(result);
    }

    #endregion

    #region Horarios por Estación

    /// <summary>
    /// Obtiene el horario semanal de una estación
    /// </summary>
    [HttpGet("estacion/{estacionId:int}")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<HorarioSemanalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HorarioSemanalDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHorarioEstacion(int estacionId)
    {
        // Verificar permisos
        if (!await PuedeAccederEstacion(estacionId))
        {
            return Forbid();
        }

        var result = await _horarioService.GetHorarioEstacionAsync(estacionId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Configura el horario semanal de una estación
    /// </summary>
    [HttpPut("estacion/{estacionId:int}")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<HorarioSemanalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HorarioSemanalDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ConfigurarHorarioEstacion(
        int estacionId,
        [FromBody] ConfigurarHorarioSemanalRequest request)
    {
        if (!await PuedeAccederEstacion(estacionId))
        {
            return Forbid();
        }

        var result = await _horarioService.ConfigurarHorarioEstacionAsync(estacionId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Horario de estación {EstacionId} configurado por {UserId}",
            estacionId, User.FindFirstValue(ClaimTypes.NameIdentifier));

        return Ok(result);
    }

    /// <summary>
    /// Cambia entre horario genérico y personalizado
    /// </summary>
    [HttpPost("estacion/{estacionId:int}/cambiar-tipo")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<HorarioSemanalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CambiarTipoHorario(
        int estacionId,
        [FromBody] CambiarTipoHorarioRequest request)
    {
        if (!await PuedeAccederEstacion(estacionId))
        {
            return Forbid();
        }

        var result = await _horarioService.CambiarTipoHorarioAsync(estacionId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Tipo de horario cambiado para estación {EstacionId} por {UserId}",
            estacionId, User.FindFirstValue(ClaimTypes.NameIdentifier));

        return Ok(result);
    }

    #endregion

    #region Horarios Especiales y Días Bloqueados

    /// <summary>
    /// Obtiene los horarios especiales de una estación
    /// </summary>
    [HttpGet("estacion/{estacionId:int}/especiales")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<HorarioDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHorariosEspeciales(int estacionId, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        if (!await PuedeAccederEstacion(estacionId))
        {
            return Forbid();
        }

        var result = await _horarioService.GetHorariosEspecialesAsync(estacionId, desde, hasta);
        return Ok(result);
    }

    /// <summary>
    /// Crea un horario especial para una estación
    /// </summary>
    [HttpPost("estacion/{estacionId:int}/especiales")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<HorarioDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<HorarioDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CrearHorarioEspecial(int estacionId, [FromBody] HorarioEspecialRequest request)
    {
        if (!await PuedeAccederEstacion(estacionId))
        {
            return Forbid();
        }

        var result = await _horarioService.CrearHorarioEspecialAsync(estacionId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Horario especial creado para estación {EstacionId} por {UserId}",
            estacionId, User.FindFirstValue(ClaimTypes.NameIdentifier));

        return CreatedAtAction(nameof(GetHorariosEspeciales), new { estacionId }, result);
    }

    /// <summary>
    /// Actualiza un horario especial
    /// </summary>
    [HttpPut("especiales/{horarioId:int}")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<HorarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HorarioDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActualizarHorarioEspecial(int horarioId, [FromBody] UpdateHorarioEspecialRequest request)
    {
        // Verificar permisos sobre el horario
        if (!await PuedeAccederHorario(horarioId))
        {
            return Forbid();
        }

        var result = await _horarioService.ActualizarHorarioEspecialAsync(horarioId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Horario especial {HorarioId} actualizado por {UserId}",
            horarioId, User.FindFirstValue(ClaimTypes.NameIdentifier));

        return Ok(result);
    }

    /// <summary>
    /// Elimina un horario especial
    /// </summary>
    [HttpDelete("especiales/{horarioId:int}")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EliminarHorarioEspecial(int horarioId)
    {
        if (!await PuedeAccederHorario(horarioId))
        {
            return Forbid();
        }

        var result = await _horarioService.EliminarHorarioEspecialAsync(horarioId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Horario especial {HorarioId} eliminado por {UserId}",
            horarioId, User.FindFirstValue(ClaimTypes.NameIdentifier));

        return Ok(result);
    }

    /// <summary>
    /// Bloquea un día para una estación
    /// </summary>
    [HttpPost("estacion/{estacionId:int}/bloquear-dia")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ApiResponse<HorarioDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BloquearDia(int estacionId, [FromBody] BloquearDiaRequest request)
    {
        if (!await PuedeAccederEstacion(estacionId))
        {
            return Forbid();
        }

        var result = await _horarioService.BloquearDiaAsync(estacionId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Día bloqueado para estación {EstacionId} por {UserId}",
            estacionId, User.FindFirstValue(ClaimTypes.NameIdentifier));

        return CreatedAtAction(nameof(GetHorariosEspeciales), new { estacionId }, result);
    }

    #endregion

    #region Consultas de Disponibilidad (Público)

    /// <summary>
    /// Obtiene el horario efectivo de una estación para una fecha
    /// </summary>
    [HttpGet("estacion/{estacionId:int}/efectivo")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<HorarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHorarioEfectivo(int estacionId, [FromQuery] DateTime fecha)
    {
        var result = await _horarioService.GetHorarioEfectivoAsync(estacionId, fecha);
        return Ok(result);
    }

    /// <summary>
    /// Verifica si una estación está disponible en una fecha y hora
    /// </summary>
    [HttpGet("estacion/{estacionId:int}/disponible")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> EstaDisponible(int estacionId, [FromQuery] DateTime fechaHora)
    {
        var result = await _horarioService.EstaDisponibleAsync(estacionId, fechaHora);
        return Ok(result);
    }

    #endregion

    #region Helpers Privados

    /// <summary>
    /// Verifica si el usuario actual puede acceder a una estación
    /// </summary>
    private async Task<bool> PuedeAccederEstacion(int estacionId)
    {
        if (User.IsInRole(ApplicationRoles.Admin))
        {
            return true;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var estacion = await _estacionService.GetByIdAsync(estacionId);

        return estacion.Success && estacion.Data?.BarberoId == userId;
    }

    /// <summary>
    /// Verifica si el usuario actual puede acceder a un horario
    /// </summary>
    private async Task<bool> PuedeAccederHorario(int horarioId)
    {
        if (User.IsInRole(ApplicationRoles.Admin))
        {
            return true;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var horario = await _horarioService.GetHorarioByIdAsync(horarioId);

        if (!horario.Success || horario.Data?.EstacionId == null)
        {
            return false;
        }

        var estacion = await _estacionService.GetByIdAsync(horario.Data.EstacionId.Value);
        return estacion.Success && estacion.Data?.BarberoId == userId;
    }

    #endregion
}


using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonAppointmentSystem.ApiService.Application.Common.Constants;
using SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.CancelReserva;
using SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.CompletarReserva;
using SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.CreateReserva;
using SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.UpdateReserva;
using SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetCitaRapida;
using SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetMisReservas;
using SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetReservaByCodigo;
using SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetReservaById;
using SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetReservasByBarbero;
using SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetReservasPaged;
using SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetSlotsDisponibles;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.Shared.DTOs.Reservas;
using SalonAppointmentSystem.Shared.Enums;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Presentation.Controllers;

/// <summary>
/// Controlador para gestión de reservas.
/// Endpoints públicos: slots disponibles, cita rápida, buscar por código
/// Endpoints autenticados: crear, cancelar, mis reservas
/// Endpoints Admin/Barbero: listar todas, completar, actualizar
/// </summary>
public class ReservasController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReservasController> _logger;

    public ReservasController(IMediator mediator, ILogger<ReservasController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #region Endpoints Públicos (Sin autenticación)

    /// <summary>
    /// Obtiene los slots disponibles para una estación, fecha y servicio
    /// </summary>
    [HttpGet("slots")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SlotsDelDiaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSlotsDisponibles(
        [FromQuery] int estacionId,
        [FromQuery] DateTime fecha,
        [FromQuery] int servicioId)
    {
        var query = new GetSlotsDisponiblesQuery
        {
            EstacionId = estacionId,
            Fecha = fecha,
            ServicioId = servicioId
        };

        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Obtiene las opciones de cita rápida (próximos slots disponibles en cualquier estación)
    /// </summary>
    [HttpGet("cita-rapida")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CitasRapidasDisponiblesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCitaRapida(
        [FromQuery] int servicioId,
        [FromQuery] int maxOpciones = 5)
    {
        var query = new GetCitaRapidaQuery
        {
            ServicioId = servicioId,
            MaxOpciones = maxOpciones
        };

        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Busca una reserva por código (para invitados)
    /// </summary>
    [HttpPost("buscar")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorCodigo([FromBody] BuscarReservaPorCodigoRequest request)
    {
        var query = new GetReservaByCodigoQuery
        {
            Codigo = request.Codigo,
            Email = request.Email
        };

        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Crear reserva como invitado (sin autenticación)
    /// </summary>
    [HttpPost("anonima")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAnonima([FromBody] CreateReservaRequest request)
    {
        var command = CreateReservaCommand.FromRequest(
            request,
            usuarioId: null,
            creadaPor: null,
            rolCreador: ApplicationRoles.Invitado);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        _logger.LogInformation(
            "Reserva anónima creada: {CodigoReserva}, Cliente={NombreCliente}, Email={Email}",
            result.Value!.CodigoReserva, request.NombreCliente, request.EmailCliente);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Cancelar reserva como invitado (requiere código y email)
    /// </summary>
    [HttpPost("cancelar-anonima")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelAnonima([FromBody] CancelReservaAnonimaRequest request)
    {
        // Primero buscar la reserva por código
        var queryReserva = new GetReservaByCodigoQuery
        {
            Codigo = request.Codigo,
            Email = request.Email
        };

        var reservaResult = await _mediator.Send(queryReserva);
        if (!reservaResult.IsSuccess)
            return BadRequest(new { error = reservaResult.Error });

        // Cancelar la reserva
        var command = new CancelReservaCommand
        {
            ReservaId = reservaResult.Value!.Id,
            MotivoCancelacion = request.Motivo,
            CanceladaPor = "Invitado",
            RolCancelador = ApplicationRoles.Invitado,
            EmailVerificacion = request.Email,
            EsCancelacionPorCodigo = true
        };

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    #endregion

    #region Endpoints Autenticados (Cualquier usuario)

    /// <summary>
    /// Crear reserva como usuario autenticado
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AppPolicies.RequireAuthenticated)]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateReservaRequest request)
    {
        var command = CreateReservaCommand.FromRequest(
            request,
            usuarioId: CurrentUserId,
            creadaPor: CurrentUserId,
            rolCreador: CurrentUserRole);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        _logger.LogInformation(
            "Reserva creada por {UserId}: {CodigoReserva}",
            CurrentUserId, result.Value!.CodigoReserva);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Obtiene una reserva por su ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = AppPolicies.RequireAuthenticated)]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetReservaByIdQuery
        {
            ReservaId = id,
            UsuarioId = CurrentUserId,
            RolUsuario = CurrentUserRole
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("permiso") == true)
                return Forbid();
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene las reservas del usuario actual (cliente)
    /// </summary>
    [HttpGet("mis-reservas")]
    [Authorize(Policy = AppPolicies.RequireAuthenticated)]
    [ProducesResponseType(typeof(List<ReservaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMisReservas(
        [FromQuery] EstadoReserva? estado = null,
        [FromQuery] bool soloFuturas = true,
        [FromQuery] bool incluirCanceladas = false)
    {
        var query = new GetMisReservasQuery
        {
            UsuarioId = CurrentUserId!,
            Estado = estado,
            SoloFuturas = soloFuturas,
            IncluirCanceladas = incluirCanceladas
        };

        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Cancelar reserva propia
    /// </summary>
    [HttpPost("{id:int}/cancelar")]
    [Authorize(Policy = AppPolicies.RequireAuthenticated)]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelReservaRequest request)
    {
        var command = new CancelReservaCommand
        {
            ReservaId = id,
            MotivoCancelacion = request.Motivo,
            CanceladaPor = CurrentUserId ?? string.Empty,
            RolCancelador = CurrentUserRole
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("permiso") == true)
                return Forbid();
            return BadRequest(new { error = result.Error });
        }

        _logger.LogInformation(
            "Reserva cancelada por {UserId}: ID={ReservaId}",
            CurrentUserId, id);

        return Ok(result.Value);
    }

    #endregion

    #region Endpoints Admin/Barbero

    /// <summary>
    /// Obtiene la agenda del barbero actual
    /// </summary>
    [HttpGet("mi-agenda")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(List<ReservaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMiAgenda(
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] bool incluirCanceladas = false)
    {
        var query = new GetReservasByBarberoQuery
        {
            BarberoId = CurrentUserId!,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            IncluirCanceladas = incluirCanceladas
        };

        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Obtiene todas las reservas con paginación y filtros (Admin)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AppPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(PagedResult<ReservaListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] GetReservasPagedQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Marca una reserva como completada
    /// </summary>
    [HttpPost("{id:int}/completar")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Completar(int id)
    {
        var command = new CompletarReservaCommand
        {
            ReservaId = id,
            CompletadaPor = CurrentUserId ?? string.Empty
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        _logger.LogInformation(
            "Reserva completada por {UserId}: ID={ReservaId}",
            CurrentUserId, id);

        return Ok(result.Value);
    }

    /// <summary>
    /// Actualiza una reserva (Admin/Barbero)
    /// </summary>
    [HttpPatch("{id:int}")]
    [Authorize(Policy = AppPolicies.RequireAdminOrBarbero)]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReservaRequest request)
    {
        var command = new UpdateReservaCommand
        {
            ReservaId = id,
            FechaHora = request.FechaHora,
            EstacionId = request.EstacionId,
            NombreCliente = request.NombreCliente,
            EmailCliente = request.EmailCliente,
            TelefonoCliente = request.TelefonoCliente,
            Estado = request.Estado,
            ActualizadaPor = CurrentUserId ?? string.Empty,
            RolActualizador = CurrentUserRole ?? string.Empty
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        _logger.LogInformation(
            "Reserva actualizada por {UserId}: ID={ReservaId}",
            CurrentUserId, id);

        return Ok(result.Value);
    }

    #endregion
}


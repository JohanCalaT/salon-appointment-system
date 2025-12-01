using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SalonAppointmentSystem.ApiService.Application.Common.Constants;

namespace SalonAppointmentSystem.ApiService.Presentation.Controllers;

/// <summary>
/// Controlador base para todos los controladores de la API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Obtiene el ID del usuario autenticado actual
    /// </summary>
    protected string? CurrentUserId => User.FindFirstValue(AppClaimTypes.UserId);

    /// <summary>
    /// Obtiene el email del usuario autenticado actual
    /// </summary>
    protected string? CurrentUserEmail => User.FindFirstValue(AppClaimTypes.Email);

    /// <summary>
    /// Obtiene el rol del usuario autenticado actual
    /// </summary>
    protected string? CurrentUserRole => User.FindFirstValue(AppClaimTypes.Role);

    /// <summary>
    /// Obtiene el ID de la estación del barbero (si aplica)
    /// </summary>
    protected int? CurrentUserEstacionId
    {
        get
        {
            var value = User.FindFirstValue(AppClaimTypes.EstacionId);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    /// <summary>
    /// Verifica si el usuario actual es administrador
    /// </summary>
    protected bool IsAdmin => User.IsInRole("Admin");

    /// <summary>
    /// Verifica si el usuario actual es barbero
    /// </summary>
    protected bool IsBarbero => User.IsInRole("Barbero");

    /// <summary>
    /// Verifica si el usuario actual es cliente
    /// </summary>
    protected bool IsCliente => User.IsInRole("Cliente");

    /// <summary>
    /// Obtiene la IP del cliente
    /// </summary>
    protected string? ClientIpAddress => 
        HttpContext.Connection.RemoteIpAddress?.ToString();
}


using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonAppointmentSystem.ApiService.Application.Common.Constants;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.Shared.DTOs.Auth;

namespace SalonAppointmentSystem.ApiService.Presentation.Controllers;

/// <summary>
/// Controlador de autenticación
/// </summary>
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Inicia sesión con email y contraseña
    /// </summary>
    /// <param name="request">Credenciales de login</param>
    /// <returns>Tokens de acceso si las credenciales son válidas</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request, ClientIpAddress);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        _logger.LogInformation("User {Email} logged in successfully", request.Email);
        return Ok(result);
    }

    /// <summary>
    /// Registra un nuevo usuario como Cliente
    /// </summary>
    /// <param name="request">Datos de registro</param>
    /// <returns>Tokens de acceso si el registro es exitoso</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request, ClientIpAddress);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("New user registered: {Email}", request.Email);
        return CreatedAtAction(nameof(Login), result);
    }

    /// <summary>
    /// Renueva el access token usando un refresh token válido
    /// </summary>
    /// <param name="request">Tokens actuales</param>
    /// <returns>Nuevos tokens si el refresh token es válido</returns>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request, ClientIpAddress);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cierra la sesión revocando el refresh token actual
    /// </summary>
    /// <param name="refreshToken">Token a revocar (en el body)</param>
    /// <returns>Ok si se revocó correctamente</returns>
    [HttpPost("logout")]
    [Authorize(Policy = AppPolicies.RequireAuthenticated)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var revoked = await _authService.RevokeTokenAsync(request.RefreshToken, ClientIpAddress);
        
        if (!revoked)
        {
            return BadRequest(new { Message = "Token inválido o ya revocado" });
        }

        _logger.LogInformation("User {UserId} logged out", CurrentUserId);
        return Ok(new { Message = "Sesión cerrada correctamente" });
    }

    /// <summary>
    /// Cierra todas las sesiones del usuario actual (revoca todos sus refresh tokens)
    /// </summary>
    /// <returns>Cantidad de sesiones cerradas</returns>
    [HttpPost("logout-all")]
    [Authorize(Policy = AppPolicies.RequireAuthenticated)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var count = await _authService.RevokeAllUserTokensAsync(userId, ClientIpAddress);
        
        _logger.LogInformation("User {UserId} logged out from all sessions ({Count} tokens)", userId, count);
        return Ok(new { Message = $"Se cerraron {count} sesiones activas" });
    }

    /// <summary>
    /// Obtiene información del usuario autenticado actual
    /// </summary>
    /// <returns>Información del usuario</returns>
    [HttpGet("me")]
    [Authorize(Policy = AppPolicies.RequireAuthenticated)]
    [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var userInfo = new UserInfo
        {
            Id = CurrentUserId ?? string.Empty,
            Email = CurrentUserEmail ?? string.Empty,
            NombreCompleto = User.FindFirstValue(AppClaimTypes.FullName) ?? string.Empty,
            Rol = CurrentUserRole ?? string.Empty,
            PuntosAcumulados = int.TryParse(User.FindFirstValue(AppClaimTypes.Puntos), out var p) ? p : 0,
            EstacionId = CurrentUserEstacionId
        };

        return Ok(userInfo);
    }
}

/// <summary>
/// DTO para la solicitud de logout
/// </summary>
public record LogoutRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}


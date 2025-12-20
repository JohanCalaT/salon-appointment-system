using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using SalonAppointmentSystem.Web.Constants;

namespace SalonAppointmentSystem.Web.Services.Auth;

/// <summary>
/// Proveedor de estado de autenticación personalizado para Blazor Server
/// Integra el sistema de autenticación JWT con el sistema nativo de Blazor
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;

    public CustomAuthenticationStateProvider(
        IAuthService authService,
        ITokenService tokenService,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
        _authService = authService;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el estado de autenticación actual del usuario
    /// Este método es llamado automáticamente por Blazor cuando se necesita verificar la autenticación
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Obtener el token actual del AuthService
            var token = await _authService.GetAccessTokenAsync();

            _logger.LogDebug("GetAuthenticationStateAsync - Token obtenido: {HasToken}", !string.IsNullOrEmpty(token));

            // Si no hay token, retornar usuario anónimo
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogDebug("No se encontró token, usuario anónimo");
                return CreateAnonymousState();
            }

            // Verificar que el token tenga formato válido
            if (!_tokenService.IsValidTokenFormat(token))
            {
                _logger.LogWarning("Token con formato inválido encontrado");
                return CreateAnonymousState();
            }

            // Verificar si el token está expirado
            if (_tokenService.IsTokenExpired(token))
            {
                _logger.LogInformation("Token expirado, intentando refresh automático");

                // Intentar renovar el token automáticamente
                var refreshResult = await _authService.RefreshTokenAsync();

                if (!refreshResult.Success || string.IsNullOrEmpty(refreshResult.AccessToken))
                {
                    _logger.LogWarning("Refresh token fallido, usuario anónimo");
                    return CreateAnonymousState();
                }

                // Usar el nuevo token
                token = refreshResult.AccessToken;
            }

            // Extraer claims del token
            var claims = _tokenService.GetClaimsFromToken(token).ToList();

            if (!claims.Any())
            {
                _logger.LogWarning("No se pudieron extraer claims del token");
                return CreateAnonymousState();
            }

            // Agregar el claim de autenticación estándar de .NET
            // Esto es importante para que IsAuthenticated funcione correctamente
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            _logger.LogDebug("Usuario autenticado: {UserId}, Rol: {Role}",
                _tokenService.GetUserId(token),
                _tokenService.GetUserRole(token));

            return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estado de autenticación");
            return CreateAnonymousState();
        }
    }

    /// <summary>
    /// Notifica a Blazor que el usuario se ha autenticado
    /// Debe ser llamado después de un login exitoso
    /// </summary>
    public void NotifyUserAuthentication()
    {
        try
        {
            var authStateTask = GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(authStateTask);
            _logger.LogInformation("Notificación de autenticación enviada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar autenticación");
        }
    }

    /// <summary>
    /// Notifica a Blazor que el usuario ha cerrado sesión
    /// Debe ser llamado después de un logout
    /// </summary>
    public void NotifyUserLogout()
    {
        try
        {
            var anonymousState = Task.FromResult(CreateAnonymousState());
            NotifyAuthenticationStateChanged(anonymousState);
            _logger.LogInformation("Notificación de logout enviada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar logout");
        }
    }

    /// <summary>
    /// Crea un estado de autenticación para un usuario anónimo (no autenticado)
    /// </summary>
    private AuthenticationState CreateAnonymousState()
    {
        var anonymousIdentity = new ClaimsIdentity();
        var anonymousUser = new ClaimsPrincipal(anonymousIdentity);
        return new AuthenticationState(anonymousUser);
    }
}


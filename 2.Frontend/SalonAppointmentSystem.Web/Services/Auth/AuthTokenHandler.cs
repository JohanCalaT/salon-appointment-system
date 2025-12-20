using System.Net.Http.Headers;

namespace SalonAppointmentSystem.Web.Services.Auth;

/// <summary>
/// DelegatingHandler que intercepta todas las peticiones HTTP salientes
/// y agrega automáticamente el token JWT en el header Authorization
/// IMPORTANTE: Este handler se ejecuta en el servidor (Blazor Server)
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthTokenHandler> _logger;

    public AuthTokenHandler(
        IAuthService authService,
        ILogger<AuthTokenHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Intercepta la petición HTTP antes de enviarla
    /// Agrega el header Authorization: Bearer {token} si existe un token válido
    /// </summary>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Obtener el token actual del AuthService
            var token = await _authService.GetAccessTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                // Agregar el token al header Authorization
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                _logger.LogDebug("Token JWT agregado a la petición: {Method} {Uri}",
                    request.Method,
                    request.RequestUri?.PathAndQuery);
            }
            else
            {
                _logger.LogDebug("No hay token disponible para la petición: {Method} {Uri}",
                    request.Method,
                    request.RequestUri?.PathAndQuery);
            }
        }
        catch (Exception ex)
        {
            // No bloquear la petición si hay error al obtener el token
            // Solo loguear el error y continuar sin el token
            _logger.LogWarning(ex, "Error al obtener token para petición HTTP: {Method} {Uri}",
                request.Method,
                request.RequestUri?.PathAndQuery);
        }

        // Continuar con la petición (con o sin token)
        return await base.SendAsync(request, cancellationToken);
    }
}


using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SalonAppointmentSystem.Shared.DTOs.Auth;
using SalonAppointmentSystem.Web.Constants;
using System.Text.Json;
using System.Net.Http.Json;

namespace SalonAppointmentSystem.Web.Services.Auth;

/// <summary>
/// Implementación del servicio de autenticación para Blazor Server
/// Utiliza ProtectedSessionStorage para almacenar tokens de forma segura en el servidor
/// IMPORTANTE: Usa HttpClient directamente (sin AuthTokenHandler) para evitar dependencia circular
/// </summary>
public class AuthService : IAuthService
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ProtectedSessionStorage sessionStorage,
        IHttpClientFactory httpClientFactory,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _sessionStorage = sessionStorage;
        _httpClientFactory = httpClientFactory;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Intentando login para usuario: {Email}", request.Email);

            // Crear HttpClient sin AuthTokenHandler para evitar dependencia circular
            var httpClient = _httpClientFactory.CreateClient("AuthClient");

            // Llamar al endpoint de login de la API
            var response = await httpClient.PostAsJsonAsync(ApiEndpoints.Login, request);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Login fallido para usuario: {Email}. Status: {Status}, Error: {Error}",
                    request.Email, response.StatusCode, errorMessage);
                return AuthResponse.Fail($"Error al iniciar sesión: {response.StatusCode}");
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (authResponse == null || !authResponse.Success)
            {
                _logger.LogWarning("Login fallido para usuario: {Email}. Razón: {Message}",
                    request.Email, authResponse?.Message ?? "Respuesta nula");
                return authResponse ?? AuthResponse.Fail("Error desconocido");
            }

            if (!authResponse.Success || string.IsNullOrEmpty(authResponse.AccessToken))
            {
                _logger.LogWarning("Login fallido: respuesta sin token para usuario: {Email}", request.Email);
                return authResponse;
            }

            // Almacenar tokens en ProtectedSessionStorage
            await StoreTokensAsync(
                authResponse.AccessToken,
                authResponse.RefreshToken ?? string.Empty,
                authResponse.ExpiresAt ?? DateTime.UtcNow.AddHours(1));

            // Almacenar información del usuario
            if (authResponse.User != null)
            {
                await StoreUserInfoAsync(authResponse.User);
            }

            _logger.LogInformation("Login exitoso para usuario: {Email}, Rol: {Role}",
                request.Email, authResponse.User?.Rol);

            return authResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el login para usuario: {Email}", request.Email);
            return AuthResponse.Fail("Error de conexión. Por favor, intenta nuevamente.");
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            var refreshToken = await GetRefreshTokenAsync();

            if (!string.IsNullOrEmpty(refreshToken))
            {
                // Intentar revocar el refresh token en el servidor
                try
                {
                    var httpClient = _httpClientFactory.CreateClient("AuthClient");
                    var logoutRequest = new { RefreshToken = refreshToken };
                    await httpClient.PostAsJsonAsync(ApiEndpoints.Logout, logoutRequest);
                    _logger.LogInformation("Refresh token revocado exitosamente");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al revocar refresh token en el servidor");
                    // Continuar con el logout local aunque falle la revocación
                }
            }

            // Limpiar almacenamiento local
            await ClearStorageAsync();
            _logger.LogInformation("Sesión cerrada y almacenamiento limpiado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el logout");
            // Asegurar que se limpie el almacenamiento aunque haya errores
            await ClearStorageAsync();
        }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>(StorageKeys.AccessToken);
            _logger.LogDebug("GetAccessTokenAsync - Success: {Success}, HasValue: {HasValue}",
                result.Success, !string.IsNullOrEmpty(result.Value));
            return result.Success ? result.Value : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener access token del storage");
            return null;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>(StorageKeys.RefreshToken);
            return result.Success ? result.Value : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener refresh token del storage");
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var token = await GetAccessTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            // Verificar que el token tenga formato válido
            if (!_tokenService.IsValidTokenFormat(token))
            {
                _logger.LogWarning("Token con formato inválido encontrado en storage");
                await ClearStorageAsync();
                return false;
            }

            // Verificar que no esté expirado
            if (_tokenService.IsTokenExpired(token))
            {
                _logger.LogInformation("Token expirado, intentando refresh automático");

                // Intentar refresh automático
                var refreshResult = await RefreshTokenAsync();
                return refreshResult.Success;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar autenticación");
            return false;
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync()
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();
            var refreshToken = await GetRefreshTokenAsync();

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("No se encontraron tokens para refresh");
                return AuthResponse.Fail("No hay sesión activa");
            }

            _logger.LogInformation("Intentando renovar access token");

            var refreshRequest = new RefreshTokenRequest
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            var httpClient = _httpClientFactory.CreateClient("AuthClient");
            var response = await httpClient.PostAsJsonAsync(ApiEndpoints.RefreshToken, refreshRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Refresh token fallido. Status: {Status}, Error: {Error}",
                    response.StatusCode, errorMessage);
                await ClearStorageAsync();
                return AuthResponse.Fail($"Error al renovar token: {response.StatusCode}");
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (authResponse == null || !authResponse.Success)
            {
                _logger.LogWarning("Refresh token fallido: {Message}", authResponse?.Message ?? "Respuesta nula");
                await ClearStorageAsync();
                return authResponse ?? AuthResponse.Fail("Error desconocido");
            }

            if (!authResponse.Success || string.IsNullOrEmpty(authResponse.AccessToken))
            {
                _logger.LogWarning("Refresh token fallido: respuesta sin token");
                await ClearStorageAsync();
                return authResponse;
            }

            // Almacenar nuevos tokens
            await StoreTokensAsync(
                authResponse.AccessToken,
                authResponse.RefreshToken ?? refreshToken, // Usar el refresh token anterior si no viene uno nuevo
                authResponse.ExpiresAt ?? DateTime.UtcNow.AddHours(1));

            _logger.LogInformation("Access token renovado exitosamente");

            return authResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el refresh token");
            await ClearStorageAsync();
            return AuthResponse.Fail("Error al renovar la sesión");
        }
    }

    public async Task<UserInfo?> GetCurrentUserInfoAsync()
    {
        try
        {
            // Primero intentar obtener del storage
            var result = await _sessionStorage.GetAsync<UserInfo>(StorageKeys.UserInfo);
            if (result.Success && result.Value != null)
            {
                return result.Value;
            }

            // Si no está en storage, extraer del token
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var userInfo = new UserInfo
            {
                Id = _tokenService.GetUserId(token) ?? string.Empty,
                Email = _tokenService.GetUserEmail(token) ?? string.Empty,
                NombreCompleto = _tokenService.GetUserFullName(token) ?? string.Empty,
                Rol = _tokenService.GetUserRole(token) ?? string.Empty,
                PuntosAcumulados = int.TryParse(_tokenService.GetClaimValue(token, AppClaimTypes.Puntos), out var puntos) ? puntos : 0,
                EstacionId = int.TryParse(_tokenService.GetClaimValue(token, AppClaimTypes.EstacionId), out var estacionId) ? estacionId : null
            };

            // Almacenar en storage para futuras consultas
            await StoreUserInfoAsync(userInfo);

            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener información del usuario");
            return null;
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Intentando registrar nuevo usuario: {Email}", request.Email);

            var httpClient = _httpClientFactory.CreateClient("AuthClient");
            var response = await httpClient.PostAsJsonAsync(ApiEndpoints.Register, request);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Registro fallido para usuario: {Email}. Status: {Status}, Error: {Error}",
                    request.Email, response.StatusCode, errorMessage);
                return AuthResponse.Fail($"Error al registrar usuario: {response.StatusCode}");
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (authResponse == null || !authResponse.Success)
            {
                _logger.LogWarning("Registro fallido para usuario: {Email}. Razón: {Message}",
                    request.Email, authResponse?.Message ?? "Respuesta nula");
                return authResponse ?? AuthResponse.Fail("Error desconocido");
            }

            if (string.IsNullOrEmpty(authResponse.AccessToken))
            {
                _logger.LogWarning("Registro fallido: respuesta sin token para usuario: {Email}", request.Email);
                return authResponse;
            }

            // Almacenar tokens
            await StoreTokensAsync(
                authResponse.AccessToken,
                authResponse.RefreshToken ?? string.Empty,
                authResponse.ExpiresAt ?? DateTime.UtcNow.AddHours(1));

            // Almacenar información del usuario
            if (authResponse.User != null)
            {
                await StoreUserInfoAsync(authResponse.User);
            }

            _logger.LogInformation("Registro exitoso para usuario: {Email}", request.Email);

            return authResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el registro para usuario: {Email}", request.Email);
            return AuthResponse.Fail("Error de conexión. Por favor, intenta nuevamente.");
        }
    }

    public async Task<DateTime?> GetTokenExpirationAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<DateTime>(StorageKeys.TokenExpiration);
            return result.Success ? result.Value : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener fecha de expiración del storage");
            return null;
        }
    }

    // ===================================================================
    // MÉTODOS PRIVADOS DE AYUDA
    // ===================================================================

    /// <summary>
    /// Almacena los tokens en ProtectedSessionStorage
    /// </summary>
    private async Task StoreTokensAsync(string accessToken, string refreshToken, DateTime expiresAt)
    {
        await _sessionStorage.SetAsync(StorageKeys.AccessToken, accessToken);
        await _sessionStorage.SetAsync(StorageKeys.RefreshToken, refreshToken);
        await _sessionStorage.SetAsync(StorageKeys.TokenExpiration, expiresAt);
    }

    /// <summary>
    /// Almacena la información del usuario en ProtectedSessionStorage
    /// </summary>
    private async Task StoreUserInfoAsync(UserInfo userInfo)
    {
        await _sessionStorage.SetAsync(StorageKeys.UserInfo, userInfo);
    }

    /// <summary>
    /// Limpia todo el almacenamiento de autenticación
    /// </summary>
    private async Task ClearStorageAsync()
    {
        try
        {
            await _sessionStorage.DeleteAsync(StorageKeys.AccessToken);
            await _sessionStorage.DeleteAsync(StorageKeys.RefreshToken);
            await _sessionStorage.DeleteAsync(StorageKeys.TokenExpiration);
            await _sessionStorage.DeleteAsync(StorageKeys.UserInfo);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al limpiar storage");
        }
    }
}


using SalonAppointmentSystem.Shared.DTOs.Auth;

namespace SalonAppointmentSystem.Web.Services.Auth;

/// <summary>
/// Servicio de autenticación para Blazor Server
/// Maneja login, logout, almacenamiento de tokens y refresh
/// IMPORTANTE: Los tokens se almacenan en ProtectedSessionStorage (servidor), NO en el navegador
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Autentica al usuario con email y contraseña
    /// Almacena los tokens en ProtectedSessionStorage si el login es exitoso
    /// </summary>
    /// <param name="request">Credenciales de login (email y password)</param>
    /// <returns>Respuesta de autenticación con tokens y datos del usuario</returns>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Cierra la sesión del usuario
    /// Revoca el refresh token en el servidor y limpia el almacenamiento local
    /// </summary>
    /// <returns>Task completado cuando se cierra la sesión</returns>
    Task LogoutAsync();

    /// <summary>
    /// Obtiene el access token actual desde ProtectedSessionStorage
    /// </summary>
    /// <returns>Access token o null si no existe o está expirado</returns>
    Task<string?> GetAccessTokenAsync();

    /// <summary>
    /// Obtiene el refresh token actual desde ProtectedSessionStorage
    /// </summary>
    /// <returns>Refresh token o null si no existe</returns>
    Task<string?> GetRefreshTokenAsync();

    /// <summary>
    /// Verifica si el usuario está autenticado
    /// Comprueba que exista un token válido y no esté expirado
    /// </summary>
    /// <returns>True si el usuario está autenticado con un token válido</returns>
    Task<bool> IsAuthenticatedAsync();

    /// <summary>
    /// Renueva el access token usando el refresh token
    /// Actualiza los tokens en ProtectedSessionStorage si el refresh es exitoso
    /// </summary>
    /// <returns>Respuesta de autenticación con nuevos tokens</returns>
    Task<AuthResponse> RefreshTokenAsync();

    /// <summary>
    /// Obtiene información del usuario actual desde el token almacenado
    /// </summary>
    /// <returns>Información del usuario o null si no está autenticado</returns>
    Task<UserInfo?> GetCurrentUserInfoAsync();

    /// <summary>
    /// Registra un nuevo usuario como Cliente
    /// Almacena los tokens en ProtectedSessionStorage si el registro es exitoso
    /// </summary>
    /// <param name="request">Datos de registro del nuevo usuario</param>
    /// <returns>Respuesta de autenticación con tokens y datos del usuario</returns>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Obtiene la fecha de expiración del token actual
    /// </summary>
    /// <returns>Fecha de expiración en UTC o null si no hay token</returns>
    Task<DateTime?> GetTokenExpirationAsync();
}


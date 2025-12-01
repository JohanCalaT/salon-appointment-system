using SalonAppointmentSystem.Shared.DTOs.Auth;

namespace SalonAppointmentSystem.ApiService.Application.Common.Interfaces;

/// <summary>
/// Interface para el servicio de autenticación
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Autentica un usuario con email y contraseña
    /// </summary>
    /// <param name="request">Credenciales de login</param>
    /// <param name="ipAddress">IP del cliente</param>
    /// <returns>Respuesta con tokens si es exitoso</returns>
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null);

    /// <summary>
    /// Registra un nuevo usuario como Cliente
    /// </summary>
    /// <param name="request">Datos de registro</param>
    /// <param name="ipAddress">IP del cliente</param>
    /// <returns>Respuesta con tokens si es exitoso</returns>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress = null);

    /// <summary>
    /// Renueva el access token usando un refresh token válido
    /// </summary>
    /// <param name="request">Tokens actuales</param>
    /// <param name="ipAddress">IP del cliente</param>
    /// <returns>Nuevos tokens si el refresh token es válido</returns>
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null);

    /// <summary>
    /// Revoca un refresh token (logout)
    /// </summary>
    /// <param name="refreshToken">Token a revocar</param>
    /// <param name="ipAddress">IP del cliente</param>
    /// <returns>True si se revocó correctamente</returns>
    Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null);

    /// <summary>
    /// Revoca todos los refresh tokens de un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="ipAddress">IP del cliente</param>
    /// <returns>Cantidad de tokens revocados</returns>
    Task<int> RevokeAllUserTokensAsync(string userId, string? ipAddress = null);
}


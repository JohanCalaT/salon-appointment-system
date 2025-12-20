using System.Security.Claims;

namespace SalonAppointmentSystem.Web.Services.Auth;

/// <summary>
/// Servicio para manejo y parseo de tokens JWT
/// IMPORTANTE: Este servicio NO valida la firma del JWT (eso lo hace la API)
/// Solo lee y extrae información del token
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Extrae todos los claims del JWT token
    /// </summary>
    /// <param name="token">JWT token en formato string</param>
    /// <returns>Colección de claims extraídos del token</returns>
    IEnumerable<Claim> GetClaimsFromToken(string token);

    /// <summary>
    /// Obtiene el valor de un claim específico del token
    /// </summary>
    /// <param name="token">JWT token en formato string</param>
    /// <param name="claimType">Tipo de claim a buscar (ej: "uid", "email", "role")</param>
    /// <returns>Valor del claim o null si no existe</returns>
    string? GetClaimValue(string token, string claimType);

    /// <summary>
    /// Verifica si el token está expirado
    /// Considera un margen de seguridad de 5 minutos antes de la expiración real
    /// </summary>
    /// <param name="token">JWT token en formato string</param>
    /// <returns>True si el token está expirado o está por expirar (menos de 5 minutos)</returns>
    bool IsTokenExpired(string token);

    /// <summary>
    /// Obtiene la fecha de expiración del token
    /// </summary>
    /// <param name="token">JWT token en formato string</param>
    /// <returns>Fecha de expiración en UTC o null si no se puede determinar</returns>
    DateTime? GetTokenExpiration(string token);

    /// <summary>
    /// Obtiene el rol del usuario desde el token
    /// </summary>
    /// <param name="token">JWT token en formato string</param>
    /// <returns>Rol del usuario (Admin, Barbero, Cliente, Invitado) o null</returns>
    string? GetUserRole(string token);

    /// <summary>
    /// Obtiene el ID del usuario desde el token
    /// </summary>
    /// <param name="token">JWT token en formato string</param>
    /// <returns>ID del usuario o null si no existe</returns>
    string? GetUserId(string token);

    /// <summary>
    /// Obtiene el email del usuario desde el token
    /// </summary>
    /// <param name="token">JWT token en formato string</param>
    /// <returns>Email del usuario o null si no existe</returns>
    string? GetUserEmail(string token);

    /// <summary>
    /// Obtiene el nombre completo del usuario desde el token
    /// </summary>
    /// <param name="token">JWT token en formato string</param>
    /// <returns>Nombre completo del usuario o null si no existe</returns>
    string? GetUserFullName(string token);

    /// <summary>
    /// Valida que el token tenga un formato JWT válido
    /// NO valida la firma, solo el formato
    /// </summary>
    /// <param name="token">JWT token en formato string</param>
    /// <returns>True si el formato es válido</returns>
    bool IsValidTokenFormat(string token);
}


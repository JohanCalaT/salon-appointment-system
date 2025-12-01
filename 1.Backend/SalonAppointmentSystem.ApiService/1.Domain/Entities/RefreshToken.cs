using SalonAppointmentSystem.ApiService.Domain.Common;

namespace SalonAppointmentSystem.ApiService.Domain.Entities;

/// <summary>
/// Entidad para almacenar refresh tokens de usuarios
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// El token de actualización (hash o valor)
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// ID del usuario al que pertenece este token
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de expiración del token (UTC)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Fecha de creación del token (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha en que fue revocado (UTC), null si está activo
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Token que reemplazó a este (cuando se hace refresh)
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Razón de revocación (si aplica)
    /// </summary>
    public string? ReasonRevoked { get; set; }

    /// <summary>
    /// IP desde donde se creó el token
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// IP desde donde se revocó el token
    /// </summary>
    public string? RevokedByIp { get; set; }

    /// <summary>
    /// Indica si el token está activo (no expirado ni revocado)
    /// </summary>
    public bool IsActive => RevokedAt == null && !IsExpired;

    /// <summary>
    /// Indica si el token ha expirado
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Revoca el token
    /// </summary>
    public void Revoke(string? reason = null, string? ip = null, string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        ReasonRevoked = reason;
        RevokedByIp = ip;
        ReplacedByToken = replacedByToken;
    }
}


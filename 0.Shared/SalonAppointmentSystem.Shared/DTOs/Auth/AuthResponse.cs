namespace SalonAppointmentSystem.Shared.DTOs.Auth;

/// <summary>
/// DTO de respuesta de autenticación con tokens
/// </summary>
public record AuthResponse
{
    /// <summary>
    /// Indica si la autenticación fue exitosa
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Token JWT de acceso
    /// </summary>
    public string? AccessToken { get; init; }

    /// <summary>
    /// Token de actualización para obtener nuevos access tokens
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Fecha de expiración del access token (UTC)
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// Información del usuario autenticado
    /// </summary>
    public UserInfo? User { get; init; }

    /// <summary>
    /// Crea una respuesta exitosa
    /// </summary>
    public static AuthResponse Ok(string accessToken, string refreshToken, DateTime expiresAt, UserInfo user)
        => new()
        {
            Success = true,
            Message = "Autenticación exitosa",
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = user
        };

    /// <summary>
    /// Crea una respuesta de error
    /// </summary>
    public static AuthResponse Fail(string message)
        => new()
        {
            Success = false,
            Message = message
        };
}

/// <summary>
/// Información básica del usuario autenticado
/// </summary>
public record UserInfo
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string NombreCompleto { get; init; } = string.Empty;
    public string Rol { get; init; } = string.Empty;
    public int PuntosAcumulados { get; init; }
    public int? EstacionId { get; init; }
}


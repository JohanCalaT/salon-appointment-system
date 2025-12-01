namespace SalonAppointmentSystem.ApiService.Application.Common.Settings;

/// <summary>
/// Configuración para JWT tokens
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Nombre de la sección en appsettings.json
    /// </summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Clave secreta para firmar los tokens (mínimo 32 caracteres)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Emisor del token (normalmente la URL de la API)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Audiencia del token (normalmente la URL del cliente)
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo de expiración del access token en minutos
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Tiempo de expiración del refresh token en días
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}


using System.ComponentModel.DataAnnotations;

namespace SalonAppointmentSystem.Shared.DTOs.Auth;

/// <summary>
/// DTO para solicitud de renovación de token
/// </summary>
public record RefreshTokenRequest
{
    /// <summary>
    /// Token de acceso actual (puede estar expirado)
    /// </summary>
    [Required(ErrorMessage = "El access token es requerido")]
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Refresh token válido
    /// </summary>
    [Required(ErrorMessage = "El refresh token es requerido")]
    public string RefreshToken { get; init; } = string.Empty;
}


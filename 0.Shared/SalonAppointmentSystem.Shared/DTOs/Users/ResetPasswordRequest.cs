using System.ComponentModel.DataAnnotations;

namespace SalonAppointmentSystem.Shared.DTOs.Users;

/// <summary>
/// DTO para resetear la contraseña de un usuario
/// </summary>
public record ResetPasswordRequest
{
    /// <summary>
    /// Nueva contraseña del usuario
    /// </summary>
    [Required(ErrorMessage = "La nueva contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string NewPassword { get; init; } = string.Empty;

    /// <summary>
    /// Confirmación de la nueva contraseña
    /// </summary>
    [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
    [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; init; } = string.Empty;
}


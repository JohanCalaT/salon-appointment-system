using System.ComponentModel.DataAnnotations;

namespace SalonAppointmentSystem.Shared.DTOs.Auth;

/// <summary>
/// DTO para registro de nuevo usuario (Cliente)
/// </summary>
public record RegisterRequest
{
    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    [Required(ErrorMessage = "El nombre completo es requerido")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
    public string NombreCompleto { get; init; } = string.Empty;

    /// <summary>
    /// Email del usuario (será también el username)
    /// </summary>
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Confirmación de contraseña
    /// </summary>
    [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; init; } = string.Empty;

    /// <summary>
    /// Teléfono del usuario (opcional)
    /// </summary>
    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    public string? PhoneNumber { get; init; }
}


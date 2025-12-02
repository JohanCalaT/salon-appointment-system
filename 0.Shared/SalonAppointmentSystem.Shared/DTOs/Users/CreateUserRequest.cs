using System.ComponentModel.DataAnnotations;

namespace SalonAppointmentSystem.Shared.DTOs.Users;

/// <summary>
/// DTO para crear un nuevo usuario (Solo Admin)
/// </summary>
public record CreateUserRequest
{
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
    /// Confirmación de la contraseña
    /// </summary>
    [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; init; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    [Required(ErrorMessage = "El nombre completo es requerido")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
    public string NombreCompleto { get; init; } = string.Empty;

    /// <summary>
    /// Teléfono del usuario (opcional)
    /// </summary>
    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Rol a asignar al usuario (Admin, Barbero, Cliente)
    /// </summary>
    [Required(ErrorMessage = "El rol es requerido")]
    public string Rol { get; init; } = string.Empty;

    /// <summary>
    /// ID de la estación a asignar (requerido si el rol es Barbero)
    /// </summary>
    public int? EstacionId { get; init; }

    /// <summary>
    /// URL de la foto de perfil (opcional)
    /// </summary>
    [Url(ErrorMessage = "El formato de la URL no es válido")]
    public string? FotoUrl { get; init; }

    /// <summary>
    /// Indica si el usuario estará activo desde el inicio
    /// </summary>
    public bool Activo { get; init; } = true;
}


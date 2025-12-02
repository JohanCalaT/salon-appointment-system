using System.ComponentModel.DataAnnotations;

namespace SalonAppointmentSystem.Shared.DTOs.Users;

/// <summary>
/// DTO para actualización completa de usuario (PUT - Solo Admin)
/// Todos los campos son requeridos excepto los opcionales
/// </summary>
public record UpdateUserRequest
{
    /// <summary>
    /// Email del usuario
    /// </summary>
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; init; } = string.Empty;

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
    /// Rol del usuario (Admin, Barbero, Cliente)
    /// </summary>
    [Required(ErrorMessage = "El rol es requerido")]
    public string Rol { get; init; } = string.Empty;

    /// <summary>
    /// ID de la estación asignada (requerido si el rol es Barbero)
    /// </summary>
    public int? EstacionId { get; init; }

    /// <summary>
    /// URL de la foto de perfil
    /// </summary>
    [Url(ErrorMessage = "El formato de la URL no es válido")]
    public string? FotoUrl { get; init; }

    /// <summary>
    /// Indica si el usuario está activo
    /// </summary>
    public bool Activo { get; init; } = true;

    /// <summary>
    /// Puntos de fidelidad acumulados
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Los puntos no pueden ser negativos")]
    public int PuntosAcumulados { get; init; }
}


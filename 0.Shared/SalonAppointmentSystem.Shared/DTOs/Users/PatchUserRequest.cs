using System.ComponentModel.DataAnnotations;

namespace SalonAppointmentSystem.Shared.DTOs.Users;

/// <summary>
/// DTO para actualización parcial de usuario (PATCH - Solo Admin)
/// Solo los campos con valor serán actualizados
/// </summary>
public record PatchUserRequest
{
    /// <summary>
    /// Email del usuario (opcional)
    /// </summary>
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string? Email { get; init; }

    /// <summary>
    /// Nombre completo del usuario (opcional)
    /// </summary>
    [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
    public string? NombreCompleto { get; init; }

    /// <summary>
    /// Teléfono del usuario (opcional)
    /// </summary>
    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Rol del usuario (Admin, Barbero, Cliente) (opcional)
    /// </summary>
    public string? Rol { get; init; }

    /// <summary>
    /// ID de la estación asignada (opcional)
    /// </summary>
    public int? EstacionId { get; init; }

    /// <summary>
    /// URL de la foto de perfil (opcional)
    /// </summary>
    [Url(ErrorMessage = "El formato de la URL no es válido")]
    public string? FotoUrl { get; init; }

    /// <summary>
    /// Indica si el usuario está activo (opcional)
    /// </summary>
    public bool? Activo { get; init; }

    /// <summary>
    /// Puntos de fidelidad acumulados (opcional)
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Los puntos no pueden ser negativos")]
    public int? PuntosAcumulados { get; init; }

    /// <summary>
    /// Nueva contraseña (opcional - para reset de contraseña)
    /// </summary>
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string? Password { get; init; }

    /// <summary>
    /// Indica si el campo EstacionId fue enviado explícitamente
    /// Permite distinguir entre "no enviado" y "enviado como null"
    /// </summary>
    public bool EstacionIdSpecified { get; init; }
}


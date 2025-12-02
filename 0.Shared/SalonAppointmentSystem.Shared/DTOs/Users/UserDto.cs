namespace SalonAppointmentSystem.Shared.DTOs.Users;

/// <summary>
/// DTO de respuesta para usuario
/// </summary>
public record UserDto
{
    /// <summary>
    /// ID único del usuario
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Email del usuario
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string NombreCompleto { get; init; } = string.Empty;

    /// <summary>
    /// Teléfono del usuario
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Rol del usuario (Admin, Barbero, Cliente)
    /// </summary>
    public string Rol { get; init; } = string.Empty;

    /// <summary>
    /// Puntos de fidelidad acumulados
    /// </summary>
    public int PuntosAcumulados { get; init; }

    /// <summary>
    /// Indica si el usuario está activo
    /// </summary>
    public bool Activo { get; init; }

    /// <summary>
    /// Fecha de registro del usuario
    /// </summary>
    public DateTime FechaRegistro { get; init; }

    /// <summary>
    /// ID de la estación asignada (solo para barberos)
    /// </summary>
    public int? EstacionId { get; init; }

    /// <summary>
    /// Nombre de la estación asignada (solo para barberos)
    /// </summary>
    public string? EstacionNombre { get; init; }

    /// <summary>
    /// URL de la foto de perfil
    /// </summary>
    public string? FotoUrl { get; init; }

    /// <summary>
    /// Indica si el email ha sido confirmado
    /// </summary>
    public bool EmailConfirmado { get; init; }
}


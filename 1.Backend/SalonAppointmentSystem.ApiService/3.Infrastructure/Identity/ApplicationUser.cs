using Microsoft.AspNetCore.Identity;
using SalonAppointmentSystem.ApiService.Domain.Entities;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Identity;

/// <summary>
/// Usuario de la aplicación extendiendo IdentityUser
/// Roles: Invitado, Cliente, Barbero, Admin
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Puntos de fidelidad acumulados
    /// </summary>
    public int PuntosAcumulados { get; set; } = 0;

    /// <summary>
    /// Indica si el usuario está activo en el sistema
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Fecha de registro del usuario
    /// </summary>
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID de la estación asignada (solo para barberos)
    /// </summary>
    public int? EstacionId { get; set; }

    /// <summary>
    /// URL de la foto de perfil del usuario
    /// </summary>
    public string? FotoUrl { get; set; }

    // Propiedades de navegación
    /// <summary>
    /// Estación asignada al barbero (null para otros roles)
    /// </summary>
    public virtual Estacion? Estacion { get; set; }

    /// <summary>
    /// Reservas realizadas por este usuario (como cliente)
    /// </summary>
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}

/// <summary>
/// Roles del sistema
/// </summary>
public static class ApplicationRoles
{
    public const string Admin = "Admin";
    public const string Barbero = "Barbero";
    public const string Cliente = "Cliente";
    public const string Invitado = "Invitado";

    public static readonly string[] TodosLosRoles = { Admin, Barbero, Cliente, Invitado };
}


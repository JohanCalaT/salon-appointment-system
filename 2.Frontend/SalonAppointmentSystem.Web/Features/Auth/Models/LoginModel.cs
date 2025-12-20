using System.ComponentModel.DataAnnotations;

namespace SalonAppointmentSystem.Web.Features.Auth.Models;

/// <summary>
/// Modelo de validación para el formulario de login
/// </summary>
public class LoginModel
{
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}


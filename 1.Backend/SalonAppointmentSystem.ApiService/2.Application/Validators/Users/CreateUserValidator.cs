using FluentValidation;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.Shared.DTOs.Users;

namespace SalonAppointmentSystem.ApiService.Application.Validators.Users;

/// <summary>
/// Validador para la creación de usuarios
/// </summary>
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    private static readonly string[] ValidRoles = { ApplicationRoles.Admin, ApplicationRoles.Barbero, ApplicationRoles.Cliente };

    public CreateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("El formato del email no es válido")
            .MaximumLength(256).WithMessage("El email no puede exceder 256 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
            .MaximumLength(100).WithMessage("La contraseña no puede exceder 100 caracteres")
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula")
            .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una minúscula")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("La confirmación de contraseña es requerida")
            .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden");

        RuleFor(x => x.NombreCompleto)
            .NotEmpty().WithMessage("El nombre completo es requerido")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres")
            .Matches(@"^[\d\s\+\-\(\)]*$").WithMessage("El teléfono contiene caracteres inválidos")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Rol)
            .NotEmpty().WithMessage("El rol es requerido")
            .Must(rol => ValidRoles.Contains(rol))
            .WithMessage($"El rol debe ser uno de: {string.Join(", ", ValidRoles)}");

        RuleFor(x => x.EstacionId)
            .NotNull().WithMessage("La estación es requerida para barberos")
            .GreaterThan(0).WithMessage("El ID de estación debe ser mayor a 0")
            .When(x => x.Rol == ApplicationRoles.Barbero);

        RuleFor(x => x.FotoUrl)
            .MaximumLength(500).WithMessage("La URL de foto no puede exceder 500 caracteres")
            .Must(BeAValidUrl).WithMessage("La URL de foto no es válida")
            .When(x => !string.IsNullOrEmpty(x.FotoUrl));
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}


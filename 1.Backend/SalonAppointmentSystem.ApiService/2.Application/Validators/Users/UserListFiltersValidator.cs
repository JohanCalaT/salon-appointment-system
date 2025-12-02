using FluentValidation;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.Shared.DTOs.Users;

namespace SalonAppointmentSystem.ApiService.Application.Validators.Users;

/// <summary>
/// Validador para los filtros de listado de usuarios
/// </summary>
public class UserListFiltersValidator : AbstractValidator<UserListFilters>
{
    private static readonly string[] ValidRoles = { ApplicationRoles.Admin, ApplicationRoles.Barbero, ApplicationRoles.Cliente };

    public UserListFiltersValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(100).WithMessage("El término de búsqueda no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Search));

        RuleFor(x => x.Rol)
            .Must(rol => ValidRoles.Contains(rol!))
            .WithMessage($"El rol debe ser uno de: {string.Join(", ", ValidRoles)}")
            .When(x => !string.IsNullOrEmpty(x.Rol));

        RuleFor(x => x.EstacionId)
            .GreaterThan(0).WithMessage("El ID de estación debe ser mayor a 0")
            .When(x => x.EstacionId.HasValue);

        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("El número de página debe ser mayor a 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100");

        RuleFor(x => x.OrderBy)
            .Must(BeValidOrderByField)
            .WithMessage($"El campo de ordenamiento debe ser uno de: {string.Join(", ", UserListFilters.ValidOrderByFields)}")
            .When(x => !string.IsNullOrEmpty(x.OrderBy));
    }

    private static bool BeValidOrderByField(string? orderBy)
    {
        if (string.IsNullOrEmpty(orderBy)) return true;
        return UserListFilters.ValidOrderByFields.Contains(orderBy, StringComparer.OrdinalIgnoreCase);
    }
}


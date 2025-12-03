using FluentValidation;
using SalonAppointmentSystem.Shared.DTOs.Estaciones;

namespace SalonAppointmentSystem.ApiService.Application.Validators.Estaciones;

/// <summary>
/// Validador para la creación de estaciones
/// </summary>
public class CreateEstacionValidator : AbstractValidator<CreateEstacionRequest>
{
    public CreateEstacionValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));

        RuleFor(x => x.Orden)
            .GreaterThanOrEqualTo(0).WithMessage("El orden debe ser mayor o igual a 0");
    }
}


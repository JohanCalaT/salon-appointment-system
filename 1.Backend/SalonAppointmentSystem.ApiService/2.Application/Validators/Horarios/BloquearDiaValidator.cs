using FluentValidation;
using SalonAppointmentSystem.Shared.DTOs.Horarios;

namespace SalonAppointmentSystem.ApiService.Application.Validators.Horarios;

/// <summary>
/// Validador para bloquear un día
/// </summary>
public class BloquearDiaValidator : AbstractValidator<BloquearDiaRequest>
{
    public BloquearDiaValidator()
    {
        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es requerida")
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("No se puede bloquear una fecha pasada");

        RuleFor(x => x.Motivo)
            .MaximumLength(500).WithMessage("El motivo no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Motivo));
    }
}


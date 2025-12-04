using FluentValidation;
using SalonAppointmentSystem.Shared.DTOs.Servicios;

namespace SalonAppointmentSystem.ApiService.Application.Validators.Servicios;

/// <summary>
/// Validador para CreateServicioRequest
/// </summary>
public class CreateServicioValidator : AbstractValidator<CreateServicioRequest>
{
    public CreateServicioValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));

        RuleFor(x => x.DuracionMinutos)
            .GreaterThan(0).WithMessage("La duración debe ser mayor a 0 minutos")
            .LessThanOrEqualTo(480).WithMessage("La duración no puede exceder 8 horas (480 minutos)");

        RuleFor(x => x.Precio)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0")
            .LessThanOrEqualTo(99999.99m).WithMessage("El precio no puede exceder 99,999.99");

        RuleFor(x => x.PuntosQueOtorga)
            .GreaterThanOrEqualTo(0).WithMessage("Los puntos no pueden ser negativos")
            .LessThanOrEqualTo(1000).WithMessage("Los puntos no pueden exceder 1000");

        RuleFor(x => x.Orden)
            .GreaterThanOrEqualTo(0).WithMessage("El orden no puede ser negativo");
    }
}


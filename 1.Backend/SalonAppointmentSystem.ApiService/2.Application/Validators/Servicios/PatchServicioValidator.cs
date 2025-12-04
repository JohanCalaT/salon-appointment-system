using FluentValidation;
using SalonAppointmentSystem.Shared.DTOs.Servicios;

namespace SalonAppointmentSystem.ApiService.Application.Validators.Servicios;

/// <summary>
/// Validador para PatchServicioRequest (actualización parcial)
/// </summary>
public class PatchServicioValidator : AbstractValidator<PatchServicioRequest>
{
    public PatchServicioValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre no puede estar vacío si se proporciona")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres")
            .When(x => x.Nombre != null);

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => x.Descripcion != null);

        RuleFor(x => x.DuracionMinutos)
            .GreaterThan(0).WithMessage("La duración debe ser mayor a 0 minutos")
            .LessThanOrEqualTo(480).WithMessage("La duración no puede exceder 8 horas (480 minutos)")
            .When(x => x.DuracionMinutos.HasValue);

        RuleFor(x => x.Precio)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0")
            .LessThanOrEqualTo(99999.99m).WithMessage("El precio no puede exceder 99,999.99")
            .When(x => x.Precio.HasValue);

        RuleFor(x => x.PuntosQueOtorga)
            .GreaterThanOrEqualTo(0).WithMessage("Los puntos no pueden ser negativos")
            .LessThanOrEqualTo(1000).WithMessage("Los puntos no pueden exceder 1000")
            .When(x => x.PuntosQueOtorga.HasValue);

        RuleFor(x => x.Orden)
            .GreaterThanOrEqualTo(0).WithMessage("El orden no puede ser negativo")
            .When(x => x.Orden.HasValue);
    }
}


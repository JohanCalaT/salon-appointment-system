using FluentValidation;
using SalonAppointmentSystem.Shared.DTOs.Horarios;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Application.Validators.Horarios;

/// <summary>
/// Validador para la creación de horarios especiales
/// </summary>
public class HorarioEspecialValidator : AbstractValidator<HorarioEspecialRequest>
{
    public HorarioEspecialValidator()
    {
        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("El tipo de horario no es válido")
            .Must(t => t != TipoHorario.Regular)
            .WithMessage("Use el endpoint de horario semanal para horarios regulares");

        RuleFor(x => x.FechaDesde)
            .NotEmpty().WithMessage("La fecha de inicio es requerida")
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("La fecha de inicio no puede ser en el pasado");

        RuleFor(x => x.FechaHasta)
            .GreaterThanOrEqualTo(x => x.FechaDesde)
            .WithMessage("La fecha de fin debe ser igual o posterior a la fecha de inicio")
            .When(x => x.FechaHasta.HasValue);

        // Validaciones específicas para horarios especiales (no bloqueados)
        When(x => x.Tipo == TipoHorario.Especial, () =>
        {
            RuleFor(x => x.HoraInicio)
                .NotNull().WithMessage("La hora de inicio es requerida para horarios especiales");

            RuleFor(x => x.HoraFin)
                .NotNull().WithMessage("La hora de fin es requerida para horarios especiales");

            RuleFor(x => x)
                .Must(x => x.HoraFin > x.HoraInicio)
                .WithMessage("La hora de fin debe ser posterior a la hora de inicio")
                .When(x => x.HoraInicio.HasValue && x.HoraFin.HasValue);
        });

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));
    }
}

/// <summary>
/// Validador para la actualización de horarios especiales
/// </summary>
public class UpdateHorarioEspecialValidator : AbstractValidator<UpdateHorarioEspecialRequest>
{
    public UpdateHorarioEspecialValidator()
    {
        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("El tipo de horario no es válido")
            .Must(t => t != TipoHorario.Regular)
            .WithMessage("Use el endpoint de horario semanal para horarios regulares");

        RuleFor(x => x.FechaDesde)
            .NotEmpty().WithMessage("La fecha de inicio es requerida");

        RuleFor(x => x.FechaHasta)
            .GreaterThanOrEqualTo(x => x.FechaDesde)
            .WithMessage("La fecha de fin debe ser igual o posterior a la fecha de inicio")
            .When(x => x.FechaHasta.HasValue);

        When(x => x.Tipo == TipoHorario.Especial, () =>
        {
            RuleFor(x => x.HoraInicio)
                .NotNull().WithMessage("La hora de inicio es requerida para horarios especiales");

            RuleFor(x => x.HoraFin)
                .NotNull().WithMessage("La hora de fin es requerida para horarios especiales");

            RuleFor(x => x)
                .Must(x => x.HoraFin > x.HoraInicio)
                .WithMessage("La hora de fin debe ser posterior a la hora de inicio")
                .When(x => x.HoraInicio.HasValue && x.HoraFin.HasValue);
        });

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));
    }
}


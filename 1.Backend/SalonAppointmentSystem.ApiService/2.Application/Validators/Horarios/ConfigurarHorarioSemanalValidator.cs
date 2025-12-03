using FluentValidation;
using SalonAppointmentSystem.Shared.DTOs.Horarios;

namespace SalonAppointmentSystem.ApiService.Application.Validators.Horarios;

/// <summary>
/// Validador para la configuración de horario semanal
/// </summary>
public class ConfigurarHorarioSemanalValidator : AbstractValidator<ConfigurarHorarioSemanalRequest>
{
    public ConfigurarHorarioSemanalValidator()
    {
        RuleFor(x => x.Dias)
            .NotNull().WithMessage("Los días son requeridos")
            .Must(dias => dias != null && dias.Count == 7)
            .WithMessage("Debe especificar los 7 días de la semana");

        RuleForEach(x => x.Dias)
            .SetValidator(new HorarioDiaRequestValidator());
    }
}

/// <summary>
/// Validador para cada día del horario semanal
/// </summary>
public class HorarioDiaRequestValidator : AbstractValidator<HorarioDiaRequest>
{
    public HorarioDiaRequestValidator()
    {
        RuleFor(x => x.DiaSemana)
            .IsInEnum().WithMessage("El día de la semana no es válido");

        When(x => x.Trabaja, () =>
        {
            RuleFor(x => x.HoraInicio)
                .NotNull().WithMessage("La hora de inicio es requerida cuando se trabaja");

            RuleFor(x => x.HoraFin)
                .NotNull().WithMessage("La hora de fin es requerida cuando se trabaja");

            RuleFor(x => x)
                .Must(x => x.HoraFin > x.HoraInicio)
                .WithMessage("La hora de fin debe ser posterior a la hora de inicio")
                .When(x => x.HoraInicio.HasValue && x.HoraFin.HasValue);
        });
    }
}


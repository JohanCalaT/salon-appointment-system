using FluentValidation;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.CompletarReserva;

/// <summary>
/// Validador para CompletarReservaCommand
/// </summary>
public class CompletarReservaCommandValidator : AbstractValidator<CompletarReservaCommand>
{
    public CompletarReservaCommandValidator()
    {
        RuleFor(x => x.ReservaId)
            .GreaterThan(0)
            .WithMessage("ID de reserva inválido");

        RuleFor(x => x.CompletadaPor)
            .NotEmpty()
            .WithMessage("Se requiere identificar quién completa la reserva");
    }
}


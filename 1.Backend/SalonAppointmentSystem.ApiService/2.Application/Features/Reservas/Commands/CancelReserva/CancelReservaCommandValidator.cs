using FluentValidation;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.CancelReserva;

/// <summary>
/// Validador para CancelReservaCommand
/// </summary>
public class CancelReservaCommandValidator : AbstractValidator<CancelReservaCommand>
{
    public CancelReservaCommandValidator()
    {
        RuleFor(x => x.ReservaId)
            .GreaterThan(0)
            .WithMessage("ID de reserva inválido");

        RuleFor(x => x.CanceladaPor)
            .NotEmpty()
            .WithMessage("Se requiere identificar quién cancela la reserva")
            .MaximumLength(450)
            .WithMessage("El identificador no puede exceder 450 caracteres");

        RuleFor(x => x.MotivoCancelacion)
            .MaximumLength(500)
            .WithMessage("El motivo no puede exceder 500 caracteres");

        // Si es cancelación por código, debe incluir email para verificación
        When(x => x.EsCancelacionPorCodigo, () =>
        {
            RuleFor(x => x.EmailVerificacion)
                .NotEmpty()
                .WithMessage("El email es requerido para verificar la cancelación")
                .EmailAddress()
                .WithMessage("El email no tiene un formato válido");
        });
    }
}


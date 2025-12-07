using FluentValidation;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.UpdateReserva;

/// <summary>
/// Validador para UpdateReservaCommand
/// </summary>
public class UpdateReservaCommandValidator : AbstractValidator<UpdateReservaCommand>
{
    public UpdateReservaCommandValidator()
    {
        RuleFor(x => x.ReservaId)
            .GreaterThan(0)
            .WithMessage("ID de reserva inválido");

        RuleFor(x => x.ActualizadaPor)
            .NotEmpty()
            .WithMessage("Se requiere identificar quién actualiza la reserva");

        RuleFor(x => x.RolActualizador)
            .NotEmpty()
            .WithMessage("Se requiere el rol del usuario que actualiza");

        // Validaciones condicionales
        When(x => x.FechaHora.HasValue, () =>
        {
            RuleFor(x => x.FechaHora!.Value)
                .GreaterThan(DateTime.UtcNow.AddMinutes(-5))
                .WithMessage("La nueva fecha debe ser futura");
        });

        When(x => x.EstacionId.HasValue, () =>
        {
            RuleFor(x => x.EstacionId!.Value)
                .GreaterThan(0)
                .WithMessage("ID de estación inválido");
        });

        When(x => !string.IsNullOrEmpty(x.NombreCliente), () =>
        {
            RuleFor(x => x.NombreCliente)
                .MaximumLength(200)
                .WithMessage("El nombre no puede exceder 200 caracteres");
        });

        When(x => !string.IsNullOrEmpty(x.EmailCliente), () =>
        {
            RuleFor(x => x.EmailCliente)
                .EmailAddress()
                .WithMessage("El email no tiene un formato válido")
                .MaximumLength(256)
                .WithMessage("El email no puede exceder 256 caracteres");
        });

        When(x => !string.IsNullOrEmpty(x.TelefonoCliente), () =>
        {
            RuleFor(x => x.TelefonoCliente)
                .MaximumLength(20)
                .WithMessage("El teléfono no puede exceder 20 caracteres")
                .Matches(@"^[\d\s\+\-\(\)]+$")
                .WithMessage("El teléfono contiene caracteres no válidos");
        });

        When(x => x.Estado.HasValue, () =>
        {
            RuleFor(x => x.Estado!.Value)
                .IsInEnum()
                .WithMessage("El estado no es válido");
        });
    }
}


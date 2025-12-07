using FluentValidation;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.CreateReserva;

/// <summary>
/// Validador para CreateReservaCommand
/// </summary>
public class CreateReservaCommandValidator : AbstractValidator<CreateReservaCommand>
{
    public CreateReservaCommandValidator()
    {
        RuleFor(x => x.EstacionId)
            .GreaterThan(0)
            .WithMessage("Debe seleccionar una estación válida");

        RuleFor(x => x.ServicioId)
            .GreaterThan(0)
            .WithMessage("Debe seleccionar un servicio válido");

        RuleFor(x => x.FechaHora)
            .GreaterThan(DateTime.UtcNow.AddMinutes(-5)) // Tolerancia de 5 min
            .WithMessage("La fecha de la reserva debe ser futura");

        RuleFor(x => x.NombreCliente)
            .NotEmpty()
            .WithMessage("El nombre del cliente es requerido")
            .MaximumLength(200)
            .WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.EmailCliente)
            .NotEmpty()
            .WithMessage("El email del cliente es requerido")
            .EmailAddress()
            .WithMessage("El email no tiene un formato válido")
            .MaximumLength(256)
            .WithMessage("El email no puede exceder 256 caracteres");

        RuleFor(x => x.TelefonoCliente)
            .NotEmpty()
            .WithMessage("El teléfono del cliente es requerido")
            .MaximumLength(20)
            .WithMessage("El teléfono no puede exceder 20 caracteres")
            .Matches(@"^[\d\s\+\-\(\)]+$")
            .WithMessage("El teléfono contiene caracteres no válidos");

        RuleFor(x => x.TipoReserva)
            .IsInEnum()
            .WithMessage("El tipo de reserva no es válido");
    }
}


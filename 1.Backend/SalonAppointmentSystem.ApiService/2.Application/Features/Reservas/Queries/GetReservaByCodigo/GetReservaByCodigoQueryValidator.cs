using FluentValidation;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Queries.GetReservaByCodigo;

/// <summary>
/// Validador para GetReservaByCodigoQuery
/// </summary>
public class GetReservaByCodigoQueryValidator : AbstractValidator<GetReservaByCodigoQuery>
{
    public GetReservaByCodigoQueryValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty()
            .WithMessage("El código de reserva es requerido")
            .Length(8)
            .WithMessage("El código de reserva debe tener 8 caracteres")
            .Matches("^[A-Z0-9]+$")
            .WithMessage("El código de reserva solo puede contener letras mayúsculas y números");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El email es requerido para verificación")
            .EmailAddress()
            .WithMessage("El email no tiene un formato válido");
    }
}


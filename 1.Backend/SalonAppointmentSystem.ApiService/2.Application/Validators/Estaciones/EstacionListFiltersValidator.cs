using FluentValidation;
using SalonAppointmentSystem.Shared.DTOs.Estaciones;

namespace SalonAppointmentSystem.ApiService.Application.Validators.Estaciones;

/// <summary>
/// Validador para los filtros de listado de estaciones
/// </summary>
public class EstacionListFiltersValidator : AbstractValidator<EstacionListFilters>
{
    private static readonly string[] ValidOrderByFields = { "nombre", "orden", "activa", "fechacreacion" };

    public EstacionListFiltersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("El número de página debe ser mayor a 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100");

        RuleFor(x => x.OrderBy)
            .Must(orderBy => string.IsNullOrEmpty(orderBy) || ValidOrderByFields.Contains(orderBy.ToLowerInvariant()))
            .WithMessage($"El campo de ordenamiento debe ser uno de: {string.Join(", ", ValidOrderByFields)}");
    }
}


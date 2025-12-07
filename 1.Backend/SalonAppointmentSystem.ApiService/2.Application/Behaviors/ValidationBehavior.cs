using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SalonAppointmentSystem.ApiService.Application.Common;

namespace SalonAppointmentSystem.ApiService.Application.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior que ejecuta validaciones FluentValidation
/// automáticamente antes de cada Command/Query
/// </summary>
/// <typeparam name="TRequest">Tipo del request (Command o Query)</typeparam>
/// <typeparam name="TResponse">Tipo de la respuesta</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var typeName = typeof(TRequest).Name;

        if (!_validators.Any())
        {
            _logger.LogDebug("No hay validadores para {RequestType}", typeName);
            return await next();
        }

        _logger.LogDebug("Ejecutando {Count} validador(es) para {RequestType}",
            _validators.Count(), typeName);

        var context = new ValidationContext<TRequest>(request);

        // Ejecutar todos los validadores en paralelo
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Recolectar errores
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = failures.Select(f => f.ErrorMessage).ToList();

            _logger.LogWarning(
                "Validación fallida para {RequestType}: {Errors}",
                typeName, string.Join(", ", errors));

            // Si el tipo de respuesta es Result<T>, retornar ValidationFailure
            if (IsResultType(typeof(TResponse)))
            {
                return CreateValidationFailureResult(errors);
            }

            // Si no es Result<T>, lanzar excepción
            throw new ValidationException(failures);
        }

        return await next();
    }

    private static bool IsResultType(Type type)
    {
        if (!type.IsGenericType)
            return type == typeof(Result);

        return type.GetGenericTypeDefinition() == typeof(Result<>);
    }

    private static TResponse CreateValidationFailureResult(List<string> errors)
    {
        var responseType = typeof(TResponse);

        // Result sin tipo genérico
        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.ValidationFailure(errors);
        }

        // Result<T>
        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var innerType = responseType.GetGenericArguments()[0];
            var method = typeof(Result<>)
                .MakeGenericType(innerType)
                .GetMethod(nameof(Result<object>.ValidationFailure))!;

            return (TResponse)method.Invoke(null, new object[] { errors })!;
        }

        throw new InvalidOperationException(
            $"No se puede crear ValidationFailure para tipo {responseType.Name}");
    }
}


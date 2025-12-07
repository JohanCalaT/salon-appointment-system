using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using SalonAppointmentSystem.ApiService.Application.Common;

namespace SalonAppointmentSystem.ApiService.Application.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior que registra información de cada request:
/// - Inicio y fin de ejecución
/// - Tiempo de ejecución
/// - Resultado (éxito/fallo)
/// - Alerta si excede umbral de tiempo
/// </summary>
/// <typeparam name="TRequest">Tipo del request</typeparam>
/// <typeparam name="TResponse">Tipo de la respuesta</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Umbral de tiempo en milisegundos para considerar una operación lenta
    /// </summary>
    private const int SlowRequestThresholdMs = 500;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid().ToString("N")[..8];

        _logger.LogInformation(
            "[{RequestId}] Iniciando {RequestName}",
            requestId, requestName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Verificar si es Result y extraer información de éxito/fallo
            var (isSuccess, error) = ExtractResultInfo(response);

            if (isSuccess)
            {
                if (elapsedMs > SlowRequestThresholdMs)
                {
                    _logger.LogWarning(
                        "[{RequestId}] {RequestName} completado en {ElapsedMs}ms (LENTO)",
                        requestId, requestName, elapsedMs);
                }
                else
                {
                    _logger.LogInformation(
                        "[{RequestId}] {RequestName} completado en {ElapsedMs}ms",
                        requestId, requestName, elapsedMs);
                }
            }
            else
            {
                _logger.LogWarning(
                    "[{RequestId}] {RequestName} falló en {ElapsedMs}ms: {Error}",
                    requestId, requestName, elapsedMs, error);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "[{RequestId}] {RequestName} lanzó excepción después de {ElapsedMs}ms",
                requestId, requestName, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    /// <summary>
    /// Extrae información de éxito/fallo si la respuesta es un Result
    /// </summary>
    private static (bool isSuccess, string? error) ExtractResultInfo(TResponse response)
    {
        if (response == null)
            return (true, null);

        var responseType = response.GetType();

        // Verificar si es Result o Result<T>
        if (responseType == typeof(Result))
        {
            var result = (Result)(object)response;
            return (result.IsSuccess, result.Error);
        }

        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var isSuccessProp = responseType.GetProperty(nameof(Result.IsSuccess));
            var errorProp = responseType.GetProperty(nameof(Result.Error));

            if (isSuccessProp != null && errorProp != null)
            {
                var isSuccess = (bool)isSuccessProp.GetValue(response)!;
                var error = (string?)errorProp.GetValue(response);
                return (isSuccess, error);
            }
        }

        // Si no es un Result, asumimos éxito
        return (true, null);
    }
}


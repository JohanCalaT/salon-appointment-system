using MediatR;
using Microsoft.Extensions.Logging;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;

namespace SalonAppointmentSystem.ApiService.Application.Behaviors;

/// <summary>
/// Marker interface para indicar que un Command requiere transacción
/// </summary>
public interface ITransactionalCommand { }

/// <summary>
/// MediatR Pipeline Behavior que envuelve Commands transaccionales
/// en una transacción de base de datos.
/// Solo aplica a Commands que implementan ITransactionalCommand.
/// </summary>
/// <typeparam name="TRequest">Tipo del request</typeparam>
/// <typeparam name="TResponse">Tipo de la respuesta</typeparam>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Solo aplicar transacción si el request implementa ITransactionalCommand
        if (request is not ITransactionalCommand)
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;

        _logger.LogDebug(
            "Iniciando transacción para {RequestName}",
            requestName);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next();

            // Verificar si el resultado fue exitoso antes de commit
            if (IsSuccessfulResult(response))
            {
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogDebug(
                    "Transacción confirmada para {RequestName}",
                    requestName);
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogDebug(
                    "Transacción revertida para {RequestName} (resultado fallido)",
                    requestName);
            }

            return response;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogError(ex,
                "Transacción revertida para {RequestName} por excepción",
                requestName);

            throw;
        }
    }

    private static bool IsSuccessfulResult(TResponse response)
    {
        if (response == null)
            return true;

        var responseType = response.GetType();

        // Verificar Result sin tipo
        if (responseType == typeof(Result))
        {
            return ((Result)(object)response).IsSuccess;
        }

        // Verificar Result<T>
        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var isSuccessProp = responseType.GetProperty(nameof(Result.IsSuccess));
            if (isSuccessProp != null)
            {
                return (bool)isSuccessProp.GetValue(response)!;
            }
        }

        // Si no es Result, asumimos éxito
        return true;
    }
}


using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.Web.Services.Http;

/// <summary>
/// Cliente HTTP genérico para comunicarse con la API
/// Utiliza Aspire Service Discovery para resolver la URL del servicio
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Realiza una petición GET
    /// </summary>
    Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza una petición GET para obtener un resultado paginado
    /// </summary>
    Task<ApiResponse<PagedResult<T>>> GetPagedAsync<T>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza una petición POST
    /// </summary>
    Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza una petición POST sin cuerpo de respuesta
    /// </summary>
    Task<ApiResponse> PostAsync<TRequest>(string endpoint, TRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza una petición PUT
    /// </summary>
    Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza una petición PUT sin cuerpo de respuesta
    /// </summary>
    Task<ApiResponse> PutAsync<TRequest>(string endpoint, TRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza una petición PATCH
    /// </summary>
    Task<ApiResponse<TResponse>> PatchAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza una petición DELETE
    /// </summary>
    Task<ApiResponse> DeleteAsync(string endpoint, CancellationToken cancellationToken = default);
}


using System.Net.Http.Json;
using System.Text.Json;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.Web.Services.Http;

/// <summary>
/// Implementación del cliente HTTP que usa Aspire Service Discovery
/// </summary>
public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            return await HandleResponseAsync<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.Fail($"Error de conexión: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResult<T>>> GetPagedAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        return await GetAsync<PagedResult<T>>(endpoint, cancellationToken);
    }

    public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data, cancellationToken);
            return await HandleResponseAsync<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiResponse<TResponse>.Fail($"Error de conexión: {ex.Message}");
        }
    }

    public async Task<ApiResponse> PostAsync<TRequest>(string endpoint, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data, cancellationToken);
            return await HandleResponseAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Error de conexión: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, data, cancellationToken);
            return await HandleResponseAsync<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiResponse<TResponse>.Fail($"Error de conexión: {ex.Message}");
        }
    }

    public async Task<ApiResponse> PutAsync<TRequest>(string endpoint, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, data, cancellationToken);
            return await HandleResponseAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Error de conexión: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TResponse>> PatchAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            var content = JsonContent.Create(data);
            var response = await _httpClient.PatchAsync(endpoint, content, cancellationToken);
            return await HandleResponseAsync<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiResponse<TResponse>.Fail($"Error de conexión: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            return await HandleResponseAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Error de conexión: {ex.Message}");
        }
    }

    private async Task<ApiResponse<T>> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (string.IsNullOrEmpty(content))
        {
            return response.IsSuccessStatusCode
                ? ApiResponse<T>.Ok(default!, "Operación exitosa")
                : ApiResponse<T>.Fail($"Error: {response.StatusCode}");
        }

        try
        {
            var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
            return result ?? ApiResponse<T>.Fail("Error al deserializar respuesta");
        }
        catch
        {
            return ApiResponse<T>.Fail($"Error al procesar respuesta: {response.StatusCode}");
        }
    }

    private async Task<ApiResponse> HandleResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (string.IsNullOrEmpty(content))
        {
            return response.IsSuccessStatusCode
                ? ApiResponse.Ok("Operación exitosa")
                : ApiResponse.Fail($"Error: {response.StatusCode}");
        }

        try
        {
            var result = JsonSerializer.Deserialize<ApiResponse>(content, _jsonOptions);
            return result ?? ApiResponse.Fail("Error al deserializar respuesta");
        }
        catch
        {
            return ApiResponse.Fail($"Error al procesar respuesta: {response.StatusCode}");
        }
    }
}


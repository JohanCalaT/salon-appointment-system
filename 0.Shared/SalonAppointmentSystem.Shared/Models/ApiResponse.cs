namespace SalonAppointmentSystem.Shared.Models;

/// <summary>
/// Respuesta estándar de la API
/// </summary>
/// <typeparam name="T">Tipo de datos de la respuesta</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Datos de la respuesta (null si hubo error)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Lista de errores de validación (si aplica)
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Timestamp de la respuesta (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Crea una respuesta exitosa
    /// </summary>
    public static ApiResponse<T> Ok(T data, string message = "Operación exitosa")
        => new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    /// <summary>
    /// Crea una respuesta de error
    /// </summary>
    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
        => new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };
}

/// <summary>
/// Respuesta estándar de la API sin datos
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Crea una respuesta exitosa sin datos
    /// </summary>
    public static ApiResponse Ok(string message = "Operación exitosa")
        => new()
        {
            Success = true,
            Message = message
        };

    /// <summary>
    /// Crea una respuesta de error sin datos
    /// </summary>
    public new static ApiResponse Fail(string message, List<string>? errors = null)
        => new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };
}


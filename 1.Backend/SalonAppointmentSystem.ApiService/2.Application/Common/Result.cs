namespace SalonAppointmentSystem.ApiService.Application.Common;

/// <summary>
/// Representa el resultado de una operación que puede fallar.
/// Patrón Result para evitar excepciones en flujo normal de negocio.
/// </summary>
/// <typeparam name="T">Tipo del valor en caso de éxito</typeparam>
public class Result<T>
{
    private Result(bool isSuccess, T? value, string? error, List<string>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ValidationErrors = validationErrors ?? new List<string>();
    }

    /// <summary>Indica si la operación fue exitosa</summary>
    public bool IsSuccess { get; }

    /// <summary>Indica si la operación falló</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Valor resultante (null si falló)</summary>
    public T? Value { get; }

    /// <summary>Mensaje de error (null si fue exitoso)</summary>
    public string? Error { get; }

    /// <summary>Lista de errores de validación</summary>
    public List<string> ValidationErrors { get; }

    /// <summary>Crea un resultado exitoso con valor</summary>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>Crea un resultado de error</summary>
    public static Result<T> Failure(string error) => new(false, default, error);

    /// <summary>Crea un resultado de error con múltiples errores de validación</summary>
    public static Result<T> ValidationFailure(List<string> errors)
        => new(false, default, "Error de validación", errors);

    /// <summary>Crea un resultado de error a partir de otro Result</summary>
    public static Result<T> Failure(Result<T> other)
        => new(false, default, other.Error, other.ValidationErrors);

    /// <summary>Mapea el valor a otro tipo si es exitoso</summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess
            ? Result<TNew>.Success(mapper(Value!))
            : Result<TNew>.Failure(Error!);
    }

    /// <summary>Conversión implícita desde T a Result exitoso</summary>
    public static implicit operator Result<T>(T value) => Success(value);
}

/// <summary>
/// Resultado de operación sin valor de retorno
/// </summary>
public class Result
{
    private Result(bool isSuccess, string? error, List<string>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors ?? new List<string>();
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public List<string> ValidationErrors { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result ValidationFailure(List<string> errors) => new(false, "Error de validación", errors);

    /// <summary>Crea un Result tipado desde un Result sin tipo</summary>
    public Result<T> ToResult<T>(T value) => IsSuccess ? Result<T>.Success(value) : Result<T>.Failure(Error!);
}


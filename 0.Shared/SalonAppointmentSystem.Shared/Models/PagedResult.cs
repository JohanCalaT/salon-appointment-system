namespace SalonAppointmentSystem.Shared.Models;

/// <summary>
/// Resultado paginado genérico para listados
/// </summary>
/// <typeparam name="T">Tipo de los elementos</typeparam>
public record PagedResult<T>
{
    /// <summary>
    /// Lista de elementos de la página actual
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>
    /// Total de elementos en la base de datos
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Número de página actual (1-based)
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total de páginas disponibles
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>
    /// Indica si hay página anterior
    /// </summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>
    /// Indica si hay página siguiente
    /// </summary>
    public bool HasNext => PageNumber < TotalPages;

    /// <summary>
    /// Crea un resultado paginado vacío
    /// </summary>
    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10)
        => new()
        {
            Items = Array.Empty<T>(),
            TotalCount = 0,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

    /// <summary>
    /// Crea un resultado paginado con datos
    /// </summary>
    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
        => new()
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
}


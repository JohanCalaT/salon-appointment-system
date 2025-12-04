namespace SalonAppointmentSystem.Shared.DTOs.Servicios;

/// <summary>
/// Filtros para listar servicios con paginación
/// </summary>
public class ServicioListFilters
{
    /// <summary>
    /// Número de página (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Cantidad de elementos por página
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Búsqueda por nombre o descripción
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filtrar por estado activo/inactivo (null = todos)
    /// </summary>
    public bool? Activo { get; set; }

    /// <summary>
    /// Precio mínimo
    /// </summary>
    public decimal? PrecioMin { get; set; }

    /// <summary>
    /// Precio máximo
    /// </summary>
    public decimal? PrecioMax { get; set; }

    /// <summary>
    /// Campo por el cual ordenar (nombre, precio, duracion, orden)
    /// </summary>
    public string? OrderBy { get; set; } = "orden";

    /// <summary>
    /// Dirección del ordenamiento (asc/desc)
    /// </summary>
    public string? OrderDirection { get; set; } = "asc";
}


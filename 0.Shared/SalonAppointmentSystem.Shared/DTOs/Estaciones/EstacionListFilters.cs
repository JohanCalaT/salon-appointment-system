namespace SalonAppointmentSystem.Shared.DTOs.Estaciones;

/// <summary>
/// Filtros para listar estaciones con paginación
/// </summary>
public class EstacionListFilters
{
    /// <summary>
    /// Número de página (1-based, default: 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Tamaño de página (default: 10, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Filtrar por nombre (búsqueda parcial)
    /// </summary>
    public string? Nombre { get; set; }

    /// <summary>
    /// Filtrar por estado activo (null = todos)
    /// </summary>
    public bool? Activa { get; set; }

    /// <summary>
    /// Filtrar por ID de barbero asignado
    /// </summary>
    public string? BarberoId { get; set; }

    /// <summary>
    /// Filtrar solo estaciones sin barbero asignado
    /// </summary>
    public bool? SinBarbero { get; set; }

    /// <summary>
    /// Filtrar por tipo de horario (genérico o personalizado)
    /// </summary>
    public bool? UsaHorarioGenerico { get; set; }

    /// <summary>
    /// Campo por el cual ordenar (Nombre, Orden, Activa)
    /// </summary>
    public string OrderBy { get; set; } = "Orden";

    /// <summary>
    /// Orden ascendente o descendente
    /// </summary>
    public bool OrderDescending { get; set; } = false;
}


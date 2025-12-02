using System.ComponentModel.DataAnnotations;

namespace SalonAppointmentSystem.Shared.DTOs.Users;

/// <summary>
/// Filtros para listado de usuarios con paginación
/// </summary>
public record UserListFilters
{
    /// <summary>
    /// Término de búsqueda (busca en email y nombre)
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Filtrar por rol específico (Admin, Barbero, Cliente)
    /// </summary>
    public string? Rol { get; init; }

    /// <summary>
    /// Filtrar por estado activo/inactivo
    /// </summary>
    public bool? Activo { get; init; }

    /// <summary>
    /// Filtrar por estación asignada (solo barberos)
    /// </summary>
    public int? EstacionId { get; init; }

    /// <summary>
    /// Número de página (1-based)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "El número de página debe ser mayor a 0")]
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Cantidad de elementos por página
    /// </summary>
    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100")]
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Campo por el cual ordenar (NombreCompleto, Email, FechaRegistro, Rol)
    /// </summary>
    public string OrderBy { get; init; } = "FechaRegistro";

    /// <summary>
    /// Indica si el orden es descendente
    /// </summary>
    public bool Descending { get; init; } = true;

    /// <summary>
    /// Campos válidos para ordenamiento
    /// </summary>
    public static readonly string[] ValidOrderByFields = 
    { 
        "NombreCompleto", 
        "Email", 
        "FechaRegistro", 
        "Rol",
        "PuntosAcumulados"
    };

    /// <summary>
    /// Valida si el campo de ordenamiento es válido
    /// </summary>
    public bool IsValidOrderBy() => 
        string.IsNullOrEmpty(OrderBy) || 
        ValidOrderByFields.Contains(OrderBy, StringComparer.OrdinalIgnoreCase);
}


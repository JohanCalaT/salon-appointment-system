using System.ComponentModel.DataAnnotations;

namespace SalonAppointmentSystem.Shared.DTOs.Estaciones;

/// <summary>
/// Request para actualizar una estación (PUT - actualización completa)
/// </summary>
public class UpdateEstacionRequest
{
    /// <summary>
    /// Nombre de la estación (requerido, único)
    /// </summary>
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción o notas sobre la estación
    /// </summary>
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }

    /// <summary>
    /// ID del barbero asignado (null para desasignar)
    /// </summary>
    public string? BarberoId { get; set; }

    /// <summary>
    /// Indica si la estación está activa
    /// </summary>
    public bool Activa { get; set; }

    /// <summary>
    /// Orden de visualización
    /// </summary>
    [Range(0, 999, ErrorMessage = "El orden debe estar entre 0 y 999")]
    public int Orden { get; set; }

    /// <summary>
    /// Si es true, usa el horario global del negocio
    /// </summary>
    public bool UsaHorarioGenerico { get; set; }
}


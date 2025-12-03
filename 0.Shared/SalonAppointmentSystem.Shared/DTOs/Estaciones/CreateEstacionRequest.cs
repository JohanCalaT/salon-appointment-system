using System.ComponentModel.DataAnnotations;

namespace SalonAppointmentSystem.Shared.DTOs.Estaciones;

/// <summary>
/// Request para crear una nueva estación
/// </summary>
public class CreateEstacionRequest
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
    /// ID del barbero a asignar (opcional)
    /// </summary>
    public string? BarberoId { get; set; }

    /// <summary>
    /// Indica si la estación está activa (default: true)
    /// </summary>
    public bool Activa { get; set; } = true;

    /// <summary>
    /// Orden de visualización (default: 0)
    /// </summary>
    [Range(0, 999, ErrorMessage = "El orden debe estar entre 0 y 999")]
    public int Orden { get; set; } = 0;

    /// <summary>
    /// Si es true, usa el horario global del negocio (default: true)
    /// </summary>
    public bool UsaHorarioGenerico { get; set; } = true;
}


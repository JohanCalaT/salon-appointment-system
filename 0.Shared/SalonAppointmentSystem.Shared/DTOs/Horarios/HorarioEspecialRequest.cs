using System.ComponentModel.DataAnnotations;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.Shared.DTOs.Horarios;

/// <summary>
/// Request para crear un horario especial o día bloqueado
/// </summary>
public class HorarioEspecialRequest
{
    /// <summary>
    /// Tipo de horario (Especial o Bloqueado)
    /// </summary>
    [Required(ErrorMessage = "El tipo es requerido")]
    public TipoHorario Tipo { get; set; } = TipoHorario.Especial;

    /// <summary>
    /// Fecha desde la cual aplica el horario especial
    /// </summary>
    [Required(ErrorMessage = "La fecha de inicio es requerida")]
    public DateTime FechaDesde { get; set; }

    /// <summary>
    /// Fecha hasta la cual aplica (null = solo ese día)
    /// </summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>
    /// Hora de inicio (requerido solo si Tipo = Especial)
    /// </summary>
    public TimeSpan? HoraInicio { get; set; }

    /// <summary>
    /// Hora de fin (requerido solo si Tipo = Especial)
    /// </summary>
    public TimeSpan? HoraFin { get; set; }

    /// <summary>
    /// Descripción o motivo del horario especial
    /// (ej: "Vacaciones", "Capacitación", "Horario extendido por evento")
    /// </summary>
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
}

/// <summary>
/// Request para actualizar un horario especial
/// </summary>
public class UpdateHorarioEspecialRequest
{
    /// <summary>
    /// Tipo de horario (Especial o Bloqueado)
    /// </summary>
    [Required(ErrorMessage = "El tipo es requerido")]
    public TipoHorario Tipo { get; set; }

    /// <summary>
    /// Fecha desde la cual aplica
    /// </summary>
    [Required(ErrorMessage = "La fecha de inicio es requerida")]
    public DateTime FechaDesde { get; set; }

    /// <summary>
    /// Fecha hasta la cual aplica
    /// </summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>
    /// Hora de inicio (requerido si Tipo = Especial)
    /// </summary>
    public TimeSpan? HoraInicio { get; set; }

    /// <summary>
    /// Hora de fin (requerido si Tipo = Especial)
    /// </summary>
    public TimeSpan? HoraFin { get; set; }

    /// <summary>
    /// Descripción o motivo
    /// </summary>
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Indica si está activo
    /// </summary>
    public bool Activo { get; set; } = true;
}


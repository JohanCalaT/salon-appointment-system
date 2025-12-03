using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.Shared.DTOs.Horarios;

/// <summary>
/// DTO de respuesta para un horario
/// </summary>
public class HorarioDto
{
    /// <summary>
    /// Identificador único del horario
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID de la estación (null si es horario global)
    /// </summary>
    public int? EstacionId { get; set; }

    /// <summary>
    /// Nombre de la estación (null si es horario global)
    /// </summary>
    public string? NombreEstacion { get; set; }

    /// <summary>
    /// Tipo de horario (Regular, Especial, Bloqueado)
    /// </summary>
    public TipoHorario Tipo { get; set; }

    /// <summary>
    /// Nombre del tipo de horario para mostrar
    /// </summary>
    public string TipoNombre => Tipo switch
    {
        TipoHorario.Regular => "Regular",
        TipoHorario.Especial => "Especial",
        TipoHorario.Bloqueado => "Bloqueado",
        _ => "Desconocido"
    };

    /// <summary>
    /// Día de la semana (0=Domingo, 6=Sábado)
    /// </summary>
    public DayOfWeek DiaSemana { get; set; }

    /// <summary>
    /// Nombre del día de la semana
    /// </summary>
    public string DiaSemanaName => DiaSemana switch
    {
        DayOfWeek.Sunday => "Domingo",
        DayOfWeek.Monday => "Lunes",
        DayOfWeek.Tuesday => "Martes",
        DayOfWeek.Wednesday => "Miércoles",
        DayOfWeek.Thursday => "Jueves",
        DayOfWeek.Friday => "Viernes",
        DayOfWeek.Saturday => "Sábado",
        _ => "Desconocido"
    };

    /// <summary>
    /// Hora de inicio (formato HH:mm)
    /// </summary>
    public TimeSpan HoraInicio { get; set; }

    /// <summary>
    /// Hora de inicio formateada
    /// </summary>
    public string HoraInicioFormateada => HoraInicio.ToString(@"hh\:mm");

    /// <summary>
    /// Hora de fin (formato HH:mm)
    /// </summary>
    public TimeSpan HoraFin { get; set; }

    /// <summary>
    /// Hora de fin formateada
    /// </summary>
    public string HoraFinFormateada => HoraFin.ToString(@"hh\:mm");

    /// <summary>
    /// Indica si el horario está activo
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Fecha desde la cual aplica (para horarios especiales)
    /// </summary>
    public DateTime? FechaVigenciaDesde { get; set; }

    /// <summary>
    /// Fecha hasta la cual aplica (para horarios especiales)
    /// </summary>
    public DateTime? FechaVigenciaHasta { get; set; }

    /// <summary>
    /// Descripción o nota del horario
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Indica si es un horario global del negocio
    /// </summary>
    public bool EsHorarioGlobal => EstacionId == null;
}


namespace SalonAppointmentSystem.Shared.DTOs.Horarios;

/// <summary>
/// DTO que representa el horario semanal completo de una estación o del negocio
/// </summary>
public class HorarioSemanalDto
{
    /// <summary>
    /// ID de la estación (null si es horario global)
    /// </summary>
    public int? EstacionId { get; set; }

    /// <summary>
    /// Nombre de la estación (null si es horario global)
    /// </summary>
    public string? NombreEstacion { get; set; }

    /// <summary>
    /// Indica si la estación usa horario genérico
    /// </summary>
    public bool UsaHorarioGenerico { get; set; }

    /// <summary>
    /// Horarios por día de la semana
    /// </summary>
    public List<HorarioDiaDto> Dias { get; set; } = new();

    /// <summary>
    /// Horarios especiales vigentes
    /// </summary>
    public List<HorarioDto> HorariosEspeciales { get; set; } = new();

    /// <summary>
    /// Días bloqueados
    /// </summary>
    public List<HorarioDto> DiasBloqueados { get; set; } = new();
}

/// <summary>
/// DTO que representa el horario de un día específico
/// </summary>
public class HorarioDiaDto
{
    /// <summary>
    /// Día de la semana
    /// </summary>
    public DayOfWeek DiaSemana { get; set; }

    /// <summary>
    /// Nombre del día
    /// </summary>
    public string DiaNombre => DiaSemana switch
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
    /// Indica si el negocio/estación trabaja este día
    /// </summary>
    public bool Trabaja { get; set; }

    /// <summary>
    /// Hora de inicio (null si no trabaja)
    /// </summary>
    public TimeSpan? HoraInicio { get; set; }

    /// <summary>
    /// Hora de fin (null si no trabaja)
    /// </summary>
    public TimeSpan? HoraFin { get; set; }

    /// <summary>
    /// Hora de inicio formateada
    /// </summary>
    public string? HoraInicioFormateada => HoraInicio?.ToString(@"hh\:mm");

    /// <summary>
    /// Hora de fin formateada
    /// </summary>
    public string? HoraFinFormateada => HoraFin?.ToString(@"hh\:mm");

    /// <summary>
    /// ID del horario (para edición)
    /// </summary>
    public int? HorarioId { get; set; }
}


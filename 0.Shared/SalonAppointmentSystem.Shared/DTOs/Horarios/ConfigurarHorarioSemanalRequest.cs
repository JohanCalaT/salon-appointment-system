using System.ComponentModel.DataAnnotations;

namespace SalonAppointmentSystem.Shared.DTOs.Horarios;

/// <summary>
/// Request para configurar el horario semanal completo de una estación o global
/// </summary>
public class ConfigurarHorarioSemanalRequest
{
    /// <summary>
    /// Configuración de cada día de la semana
    /// </summary>
    [Required(ErrorMessage = "Debe especificar al menos un día")]
    public List<HorarioDiaRequest> Dias { get; set; } = new();
}

/// <summary>
/// Request para configurar un día específico del horario semanal
/// </summary>
public class HorarioDiaRequest
{
    /// <summary>
    /// Día de la semana (0=Domingo, 6=Sábado)
    /// </summary>
    [Range(0, 6, ErrorMessage = "El día debe estar entre 0 (Domingo) y 6 (Sábado)")]
    public DayOfWeek DiaSemana { get; set; }

    /// <summary>
    /// Indica si se trabaja este día
    /// </summary>
    public bool Trabaja { get; set; }

    /// <summary>
    /// Hora de inicio (requerido si Trabaja = true)
    /// Formato: "HH:mm" o TimeSpan
    /// </summary>
    public TimeSpan? HoraInicio { get; set; }

    /// <summary>
    /// Hora de fin (requerido si Trabaja = true)
    /// Formato: "HH:mm" o TimeSpan
    /// </summary>
    public TimeSpan? HoraFin { get; set; }
}


namespace SalonAppointmentSystem.Shared.Enums;

/// <summary>
/// Tipos de horario para las estaciones
/// </summary>
public enum TipoHorario
{
    /// <summary>
    /// Horario semanal regular (Lunes a Domingo)
    /// </summary>
    Regular = 0,

    /// <summary>
    /// Horario especial para una fecha o rango de fechas específico
    /// (vacaciones, eventos, horario extendido, etc.)
    /// </summary>
    Especial = 1,

    /// <summary>
    /// Día bloqueado/no disponible
    /// (día libre, enfermedad, capacitación, etc.)
    /// </summary>
    Bloqueado = 2
}


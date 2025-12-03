using SalonAppointmentSystem.ApiService.Domain.Common;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Domain.Entities;

/// <summary>
/// Define los horarios de atención de la barbería o de una estación específica
/// </summary>
public class ConfiguracionHorario : BaseEntity
{
    /// <summary>
    /// ID de la estación a la que pertenece este horario.
    /// Si es null, es un horario global del negocio.
    /// </summary>
    public int? EstacionId { get; set; }

    /// <summary>
    /// Tipo de horario (Regular, Especial, Bloqueado)
    /// </summary>
    public TipoHorario Tipo { get; set; } = TipoHorario.Regular;

    /// <summary>
    /// Día de la semana (0=Domingo, 6=Sábado)
    /// </summary>
    public DayOfWeek DiaSemana { get; set; }

    /// <summary>
    /// Hora de inicio de atención
    /// </summary>
    public TimeSpan HoraInicio { get; set; }

    /// <summary>
    /// Hora de fin de atención
    /// </summary>
    public TimeSpan HoraFin { get; set; }

    /// <summary>
    /// Indica si este horario está activo
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Fecha desde la cual aplica este horario (para horarios especiales)
    /// </summary>
    public DateTime? FechaVigenciaDesde { get; set; }

    /// <summary>
    /// Fecha hasta la cual aplica este horario (para horarios especiales)
    /// </summary>
    public DateTime? FechaVigenciaHasta { get; set; }

    /// <summary>
    /// Nota o descripción del horario especial (ej: "Vacaciones", "Evento especial")
    /// </summary>
    public string? Descripcion { get; set; }

    // Propiedades de navegación (virtual para lazy loading)
    /// <summary>
    /// Estación a la que pertenece este horario (null si es global)
    /// </summary>
    public virtual Estacion? Estacion { get; set; }

    // Métodos de dominio
    /// <summary>
    /// Verifica si el horario es válido (HoraFin > HoraInicio, excepto para bloqueados)
    /// </summary>
    public bool EsHorarioValido()
    {
        // Para horarios bloqueados no se requiere validar horas
        if (Tipo == TipoHorario.Bloqueado)
            return true;

        return HoraFin > HoraInicio;
    }

    /// <summary>
    /// Verifica si una hora específica está dentro del horario de atención
    /// </summary>
    public bool EstaEnHorario(TimeSpan hora)
    {
        // Si está bloqueado, nunca está en horario
        if (Tipo == TipoHorario.Bloqueado)
            return false;

        return hora >= HoraInicio && hora < HoraFin;
    }

    /// <summary>
    /// Verifica si este horario aplica para una fecha específica
    /// </summary>
    public bool AplicaParaFecha(DateTime fecha)
    {
        // Verificar día de la semana (solo para horarios regulares)
        if (Tipo == TipoHorario.Regular && fecha.DayOfWeek != DiaSemana)
            return false;

        // Para horarios especiales o bloqueados, verificar el rango de fechas
        if (Tipo != TipoHorario.Regular)
        {
            if (!FechaVigenciaDesde.HasValue)
                return false;

            var fechaSolo = fecha.Date;
            if (fechaSolo < FechaVigenciaDesde.Value.Date)
                return false;

            if (FechaVigenciaHasta.HasValue && fechaSolo > FechaVigenciaHasta.Value.Date)
                return false;

            return true;
        }

        // Para horarios regulares sin fechas de vigencia, aplica siempre
        if (!FechaVigenciaDesde.HasValue && !FechaVigenciaHasta.HasValue)
            return true;

        // Verificar rango de vigencia para horarios regulares con fechas
        var fechaSoloRegular = fecha.Date;
        if (FechaVigenciaDesde.HasValue && fechaSoloRegular < FechaVigenciaDesde.Value.Date)
            return false;

        if (FechaVigenciaHasta.HasValue && fechaSoloRegular > FechaVigenciaHasta.Value.Date)
            return false;

        return true;
    }

    /// <summary>
    /// Indica si es un horario global del negocio
    /// </summary>
    public bool EsHorarioGlobal => EstacionId == null;

    /// <summary>
    /// Indica si es un horario de día bloqueado
    /// </summary>
    public bool EsDiaBloqueado => Tipo == TipoHorario.Bloqueado;
}


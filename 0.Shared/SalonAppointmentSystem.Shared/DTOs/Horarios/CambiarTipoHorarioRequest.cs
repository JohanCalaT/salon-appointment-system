namespace SalonAppointmentSystem.Shared.DTOs.Horarios;

/// <summary>
/// Request para cambiar entre horario genérico y personalizado
/// </summary>
public class CambiarTipoHorarioRequest
{
    /// <summary>
    /// Si es true, la estación usará el horario global del negocio.
    /// Si es false, usará sus propios horarios personalizados.
    /// </summary>
    public bool UsaHorarioGenerico { get; set; }

    /// <summary>
    /// Si se cambia a horario personalizado y está en true,
    /// se copiarán los horarios globales actuales como base.
    /// </summary>
    public bool CopiarHorariosGlobales { get; set; } = true;
}


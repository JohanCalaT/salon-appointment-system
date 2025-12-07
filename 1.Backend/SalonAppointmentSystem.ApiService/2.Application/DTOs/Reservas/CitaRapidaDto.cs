namespace SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;

/// <summary>
/// DTO que representa la próxima cita rápida disponible
/// </summary>
public record CitaRapidaDto
{
    /// <summary>
    /// ID de la estación donde está disponible
    /// </summary>
    public int EstacionId { get; init; }
    
    /// <summary>
    /// Nombre de la estación
    /// </summary>
    public string EstacionNombre { get; init; } = string.Empty;
    
    /// <summary>
    /// Nombre del barbero asignado (si tiene)
    /// </summary>
    public string? BarberoNombre { get; init; }
    
    /// <summary>
    /// Fecha y hora del próximo slot disponible
    /// </summary>
    public DateTime FechaHora { get; init; }
    
    /// <summary>
    /// Hora formateada para mostrar
    /// </summary>
    public string HoraFormateada => FechaHora.ToString("HH:mm");
    
    /// <summary>
    /// Fecha formateada para mostrar
    /// </summary>
    public string FechaFormateada => FechaHora.ToString("dd/MM/yyyy");
    
    /// <summary>
    /// Tiempo de espera estimado en minutos desde ahora
    /// </summary>
    public int TiempoEsperaMinutos { get; init; }
    
    /// <summary>
    /// Tiempo de espera formateado
    /// </summary>
    public string TiempoEsperaFormateado
    {
        get
        {
            if (TiempoEsperaMinutos < 60)
                return $"{TiempoEsperaMinutos} min";
            
            var horas = TiempoEsperaMinutos / 60;
            var minutos = TiempoEsperaMinutos % 60;
            
            if (minutos == 0)
                return $"{horas}h";
            
            return $"{horas}h {minutos}min";
        }
    }
}

/// <summary>
/// Lista de opciones de cita rápida ordenadas por tiempo de espera
/// </summary>
public record CitasRapidasDisponiblesDto
{
    /// <summary>
    /// Lista de opciones disponibles
    /// </summary>
    public List<CitaRapidaDto> Opciones { get; init; } = new();
    
    /// <summary>
    /// La mejor opción (menor tiempo de espera)
    /// </summary>
    public CitaRapidaDto? MejorOpcion => Opciones.MinBy(o => o.TiempoEsperaMinutos);
    
    /// <summary>
    /// Indica si hay opciones disponibles
    /// </summary>
    public bool HayDisponibilidad => Opciones.Count > 0;
}


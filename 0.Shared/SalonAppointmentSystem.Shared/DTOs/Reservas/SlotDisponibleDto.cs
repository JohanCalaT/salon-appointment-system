namespace SalonAppointmentSystem.Shared.DTOs.Reservas;

/// <summary>
/// DTO que representa un slot de tiempo disponible para reservar
/// </summary>
public record SlotDisponibleDto
{
    /// <summary>
    /// Fecha y hora del slot en UTC
    /// </summary>
    public DateTime FechaHora { get; init; }
    
    /// <summary>
    /// Hora formateada para mostrar (ej: "10:00")
    /// </summary>
    public string HoraFormateada { get; init; } = string.Empty;
    
    /// <summary>
    /// Indica si el slot está disponible
    /// </summary>
    public bool Disponible { get; init; }
    
    /// <summary>
    /// Razón por la que no está disponible (si aplica)
    /// </summary>
    public string? RazonNoDisponible { get; init; }
}

/// <summary>
/// DTO con los slots disponibles de un día
/// </summary>
public record SlotsDelDiaDto
{
    /// <summary>
    /// Fecha del día
    /// </summary>
    public DateTime Fecha { get; init; }
    
    /// <summary>
    /// Lista de slots disponibles
    /// </summary>
    public List<SlotDisponibleDto> Slots { get; init; } = new();
    
    /// <summary>
    /// Indica si hay al menos un slot disponible
    /// </summary>
    public bool TieneSlotsDisponibles => Slots.Any(s => s.Disponible);
}


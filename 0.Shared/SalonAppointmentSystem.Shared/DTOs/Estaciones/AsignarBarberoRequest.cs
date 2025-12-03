namespace SalonAppointmentSystem.Shared.DTOs.Estaciones;

/// <summary>
/// Request para asignar o desasignar un barbero a una estación
/// </summary>
public class AsignarBarberoRequest
{
    /// <summary>
    /// ID del barbero a asignar. Null para desasignar el barbero actual.
    /// </summary>
    public string? BarberoId { get; set; }
}


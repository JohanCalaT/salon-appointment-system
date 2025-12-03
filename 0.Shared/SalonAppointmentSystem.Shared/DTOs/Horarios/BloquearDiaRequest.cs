namespace SalonAppointmentSystem.Shared.DTOs.Horarios;

/// <summary>
/// Request para bloquear un día completo
/// </summary>
public record BloquearDiaRequest
{
    /// <summary>
    /// Fecha a bloquear
    /// </summary>
    public DateTime Fecha { get; init; }

    /// <summary>
    /// Motivo del bloqueo (vacaciones, enfermedad, formación, etc.)
    /// </summary>
    public string? Motivo { get; init; }
}


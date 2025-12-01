namespace SalonAppointmentSystem.Shared.Enums;

/// <summary>
/// Tipos de reserva según cómo fue creada
/// </summary>
public enum TipoReserva
{
    /// <summary>
    /// Reserva creada manualmente seleccionando fecha y hora
    /// </summary>
    Manual = 0,

    /// <summary>
    /// Reserva creada con la opción de cita rápida (próximo slot disponible)
    /// </summary>
    CitaRapida = 1
}


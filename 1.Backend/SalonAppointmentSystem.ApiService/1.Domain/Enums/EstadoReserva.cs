namespace SalonAppointmentSystem.ApiService.Domain.Enums;

/// <summary>
/// Estados posibles de una reserva
/// </summary>
public enum EstadoReserva
{
    /// <summary>
    /// Reserva creada pero pendiente de confirmación
    /// </summary>
    Pendiente = 0,

    /// <summary>
    /// Reserva confirmada y lista para atender
    /// </summary>
    Confirmada = 1,

    /// <summary>
    /// Reserva completada (servicio realizado)
    /// </summary>
    Completada = 2,

    /// <summary>
    /// Reserva cancelada
    /// </summary>
    Cancelada = 3
}


namespace SalonAppointmentSystem.ApiService.Domain.Exceptions;

/// <summary>
/// Excepción para cuando hay solapamiento de horarios en reservas
/// </summary>
public class SolapamientoException : BusinessRuleException
{
    public int EstacionId { get; }
    public DateTime FechaHora { get; }

    public SolapamientoException()
        : base("Existe un solapamiento de horario con otra reserva.", "SOLAPAMIENTO_HORARIO")
    {
    }

    public SolapamientoException(string message)
        : base(message, "SOLAPAMIENTO_HORARIO")
    {
    }

    public SolapamientoException(int estacionId, DateTime fechaHora)
        : base($"La estación {estacionId} ya tiene una reserva para el horario {fechaHora:dd/MM/yyyy HH:mm}.", "SOLAPAMIENTO_HORARIO")
    {
        EstacionId = estacionId;
        FechaHora = fechaHora;
    }

    public SolapamientoException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}


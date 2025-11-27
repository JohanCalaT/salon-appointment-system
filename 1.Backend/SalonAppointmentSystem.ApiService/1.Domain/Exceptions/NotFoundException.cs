namespace SalonAppointmentSystem.ApiService.Domain.Exceptions;

/// <summary>
/// Excepción para cuando no se encuentra una entidad
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException()
        : base()
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"La entidad \"{entityName}\" con identificador ({key}) no fue encontrada.")
    {
    }
}


using MediatR;

namespace SalonAppointmentSystem.ApiService.Domain.Common;

/// <summary>
/// Interface para eventos de dominio
/// Los eventos de dominio se usan para notificar cambios importantes en el sistema
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}

/// <summary>
/// Clase base abstracta para eventos de dominio
/// </summary>
public abstract class DomainEventBase : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}


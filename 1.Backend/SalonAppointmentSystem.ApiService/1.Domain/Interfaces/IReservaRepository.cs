using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Domain.Enums;

namespace SalonAppointmentSystem.ApiService.Domain.Interfaces;

/// <summary>
/// Repositorio específico para Reservas
/// </summary>
public interface IReservaRepository : IRepository<Reserva>
{
    /// <summary>
    /// Obtiene las reservas de una estación para un rango de fechas
    /// </summary>
    Task<IReadOnlyList<Reserva>> GetByEstacionYFechaAsync(
        int estacionId,
        DateTime fechaInicio,
        DateTime fechaFin,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las reservas de un usuario
    /// </summary>
    Task<IReadOnlyList<Reserva>> GetByUsuarioIdAsync(
        string usuarioId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las reservas por email del cliente
    /// </summary>
    Task<IReadOnlyList<Reserva>> GetByEmailClienteAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si hay solapamiento de horario en una estación
    /// </summary>
    Task<bool> ExisteSolapamientoAsync(
        int estacionId,
        DateTime fechaHora,
        int duracionMinutos,
        int? excludeReservaId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las reservas futuras no canceladas de una estación
    /// </summary>
    Task<IReadOnlyList<Reserva>> GetReservasFuturasAsync(
        int estacionId,
        DateTime desde,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene reservas por estado
    /// </summary>
    Task<IReadOnlyList<Reserva>> GetByEstadoAsync(
        EstadoReserva estado,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la reserva con sus entidades relacionadas
    /// </summary>
    Task<Reserva?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken = default);
}


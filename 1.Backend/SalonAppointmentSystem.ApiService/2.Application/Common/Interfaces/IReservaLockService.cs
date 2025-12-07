namespace SalonAppointmentSystem.ApiService.Application.Common.Interfaces;

/// <summary>
/// Servicio para locks distribuidos en reservas usando Redis.
/// Previene race conditions cuando múltiples usuarios intentan reservar el mismo slot.
/// </summary>
public interface IReservaLockService
{
    /// <summary>
    /// Adquiere un lock distribuido para una estación en un rango de tiempo específico.
    /// El lock cubre todos los slots de 15 minutos afectados por la reserva.
    /// </summary>
    /// <param name="estacionId">ID de la estación</param>
    /// <param name="fechaHora">Fecha y hora de inicio de la reserva</param>
    /// <param name="duracionMinutos">Duración de la reserva en minutos</param>
    /// <param name="waitTime">Tiempo máximo de espera para adquirir el lock (default: 10s)</param>
    /// <param name="lockTime">Tiempo que se mantiene el lock (default: 30s)</param>
    /// <returns>
    /// IAsyncDisposable que libera el lock al hacer Dispose, o null si no se pudo adquirir.
    /// Usar con 'await using' para garantizar liberación del lock.
    /// </returns>
    /// <example>
    /// <code>
    /// await using var lockHandle = await _lockService.AcquireLockAsync(estacionId, fechaHora, duracion);
    /// if (lockHandle == null)
    /// {
    ///     return Error("El horario está siendo reservado por otro usuario");
    /// }
    /// // Realizar operación protegida...
    /// // Lock se libera automáticamente al salir del bloque
    /// </code>
    /// </example>
    Task<IAsyncDisposable?> AcquireLockAsync(
        int estacionId,
        DateTime fechaHora,
        int duracionMinutos,
        TimeSpan? waitTime = null,
        TimeSpan? lockTime = null);
}


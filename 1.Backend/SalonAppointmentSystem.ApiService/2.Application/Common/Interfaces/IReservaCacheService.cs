namespace SalonAppointmentSystem.ApiService.Application.Common.Interfaces;

/// <summary>
/// DTO para representar un slot de tiempo disponible
/// </summary>
public record SlotDisponibleDto
{
    /// <summary>Fecha y hora UTC del slot</summary>
    public DateTime FechaHora { get; init; }

    /// <summary>Hora formateada para mostrar (ej: "10:00")</summary>
    public string HoraFormateada { get; init; } = string.Empty;

    /// <summary>Indica si el slot está disponible para reservar</summary>
    public bool Disponible { get; init; }
}

/// <summary>
/// Servicio de cache para reservas usando Redis.
/// Almacena slots disponibles y reservas del día para reducir consultas a BD.
/// </summary>
public interface IReservaCacheService
{
    #region Cache de Slots Disponibles

    /// <summary>
    /// Obtiene los slots disponibles cacheados para una estación, fecha y servicio específicos.
    /// </summary>
    /// <param name="estacionId">ID de la estación</param>
    /// <param name="fecha">Fecha a consultar</param>
    /// <param name="servicioId">ID del servicio (afecta la duración)</param>
    /// <returns>Lista de slots disponibles, o null si no hay cache</returns>
    Task<List<SlotDisponibleDto>?> GetSlotsDisponiblesAsync(
        int estacionId,
        DateTime fecha,
        int servicioId);

    /// <summary>
    /// Guarda en cache los slots disponibles.
    /// </summary>
    /// <param name="estacionId">ID de la estación</param>
    /// <param name="fecha">Fecha de los slots</param>
    /// <param name="servicioId">ID del servicio</param>
    /// <param name="slots">Lista de slots a cachear</param>
    /// <param name="expiration">Tiempo de expiración (default: 5 minutos)</param>
    Task SetSlotsDisponiblesAsync(
        int estacionId,
        DateTime fecha,
        int servicioId,
        List<SlotDisponibleDto> slots,
        TimeSpan? expiration = null);

    /// <summary>
    /// Invalida el cache de slots para una estación y fecha.
    /// Debe llamarse después de crear o cancelar una reserva.
    /// </summary>
    /// <param name="estacionId">ID de la estación</param>
    /// <param name="fecha">Fecha a invalidar</param>
    Task InvalidateSlotsAsync(int estacionId, DateTime fecha);

    #endregion

    #region Cache de Reservas del Día

    /// <summary>
    /// Obtiene las reservas del día cacheadas para una estación.
    /// </summary>
    /// <param name="estacionId">ID de la estación</param>
    /// <param name="fecha">Fecha a consultar</param>
    /// <returns>Lista de reservas simplificadas, o null si no hay cache</returns>
    Task<List<ReservaCacheDto>?> GetReservasDelDiaAsync(int estacionId, DateTime fecha);

    /// <summary>
    /// Guarda en cache las reservas del día.
    /// </summary>
    Task SetReservasDelDiaAsync(
        int estacionId,
        DateTime fecha,
        List<ReservaCacheDto> reservas,
        TimeSpan? expiration = null);

    /// <summary>
    /// Invalida el cache de reservas para una estación y fecha.
    /// </summary>
    Task InvalidateReservasDelDiaAsync(int estacionId, DateTime fecha);

    #endregion
}

/// <summary>
/// DTO simplificado de reserva para cache (solo datos necesarios para cálculos)
/// </summary>
public record ReservaCacheDto
{
    public int Id { get; init; }
    public DateTime FechaHora { get; init; }
    public int DuracionMinutos { get; init; }
    public int Estado { get; init; }
}


using Microsoft.Extensions.Logging;
using RedLockNet;
using RedLockNet.SERedis;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Services.Redis;

/// <summary>
/// Implementación de lock distribuido para reservas usando RedLock (Redis).
/// Previene que dos usuarios reserven el mismo slot simultáneamente.
/// </summary>
public class ReservaLockService : IReservaLockService
{
    private readonly IDistributedLockFactory _lockFactory;
    private readonly ILogger<ReservaLockService> _logger;

    // Configuración por defecto
    private static readonly TimeSpan DefaultWaitTime = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan DefaultLockTime = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DefaultRetryTime = TimeSpan.FromMilliseconds(200);

    public ReservaLockService(
        IDistributedLockFactory lockFactory,
        ILogger<ReservaLockService> logger)
    {
        _lockFactory = lockFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IAsyncDisposable?> AcquireLockAsync(
        int estacionId,
        DateTime fechaHora,
        int duracionMinutos,
        TimeSpan? waitTime = null,
        TimeSpan? lockTime = null)
    {
        // Generar keys para todos los slots de 15 minutos afectados
        var lockKeys = GenerateLockKeys(estacionId, fechaHora, duracionMinutos);

        var actualWaitTime = waitTime ?? DefaultWaitTime;
        var actualLockTime = lockTime ?? DefaultLockTime;

        _logger.LogDebug(
            "Intentando adquirir lock para estación {EstacionId}, " +
            "hora: {FechaHora:yyyy-MM-dd HH:mm}, slots: {SlotsCount}, waitTime: {WaitTime}s",
            estacionId,
            fechaHora,
            lockKeys.Count,
            actualWaitTime.TotalSeconds);

        try
        {
            // Adquirir lock múltiple (todos los slots o ninguno)
            // Usamos una clave combinada para representar el rango de tiempo
            var lockKey = string.Join("|", lockKeys);
            var redLock = await _lockFactory.CreateLockAsync(
                lockKey,
                actualLockTime,
                actualWaitTime,
                DefaultRetryTime);

            if (redLock.IsAcquired)
            {
                _logger.LogDebug(
                    "Lock adquirido exitosamente para estación {EstacionId}, hora: {FechaHora:HH:mm}",
                    estacionId, fechaHora);

                return new LockHandle(redLock, _logger, estacionId, fechaHora);
            }

            _logger.LogWarning(
                "No se pudo adquirir lock para estación {EstacionId}, hora: {FechaHora:HH:mm}. " +
                "Otro proceso está reservando este horario.",
                estacionId, fechaHora);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al intentar adquirir lock para estación {EstacionId}",
                estacionId);
            throw;
        }
    }

    /// <summary>
    /// Genera las claves de lock para cada slot de 15 minutos afectado.
    /// Formato: "lock:reserva:est:{estacionId}:slot:{yyyyMMddHHmm}"
    /// </summary>
    private static List<string> GenerateLockKeys(
        int estacionId,
        DateTime fechaHora,
        int duracionMinutos)
    {
        var keys = new List<string>();
        var slotInicio = RoundToSlot(fechaHora);
        var slotFin = RoundToSlot(fechaHora.AddMinutes(duracionMinutos));

        // Si la duración no completa un slot, asegurar al menos un slot
        if (slotFin <= slotInicio)
        {
            slotFin = slotInicio.AddMinutes(15);
        }

        for (var slot = slotInicio; slot < slotFin; slot = slot.AddMinutes(15))
        {
            keys.Add($"lock:reserva:est:{estacionId}:slot:{slot:yyyyMMddHHmm}");
        }

        return keys;
    }

    /// <summary>
    /// Redondea al slot de 15 minutos más cercano hacia abajo.
    /// Ejemplo: 10:23 → 10:15, 10:45 → 10:45
    /// </summary>
    private static DateTime RoundToSlot(DateTime dateTime)
    {
        var minutes = dateTime.Minute;
        var roundedMinutes = (minutes / 15) * 15;
        return new DateTime(
            dateTime.Year, dateTime.Month, dateTime.Day,
            dateTime.Hour, roundedMinutes, 0, dateTime.Kind);
    }

    /// <summary>
    /// Wrapper para el lock que implementa IAsyncDisposable
    /// </summary>
    private sealed class LockHandle : IAsyncDisposable
    {
        private readonly IRedLock _redLock;
        private readonly ILogger _logger;
        private readonly int _estacionId;
        private readonly DateTime _fechaHora;
        private bool _disposed;

        public LockHandle(IRedLock redLock, ILogger logger, int estacionId, DateTime fechaHora)
        {
            _redLock = redLock;
            _logger = logger;
            _estacionId = estacionId;
            _fechaHora = fechaHora;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                await _redLock.DisposeAsync();
                _logger.LogDebug(
                    "Lock liberado para estación {EstacionId}, hora: {FechaHora:HH:mm}",
                    _estacionId, _fechaHora);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Error al liberar lock para estación {EstacionId}",
                    _estacionId);
            }
        }
    }
}


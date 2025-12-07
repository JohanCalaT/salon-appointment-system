using System.Text.Json;
using Microsoft.Extensions.Logging;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using StackExchange.Redis;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Services.Redis;

/// <summary>
/// Implementación de cache para reservas usando Redis.
/// Almacena slots disponibles y reservas del día para optimizar consultas.
/// </summary>
public class ReservaCacheService : IReservaCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<ReservaCacheService> _logger;

    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);
    private const string SlotsKeyPrefix = "cache:slots";
    private const string ReservasKeyPrefix = "cache:reservas";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ReservaCacheService(
        IConnectionMultiplexer redis,
        ILogger<ReservaCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    #region Slots Disponibles

    public async Task<List<SlotDisponibleDto>?> GetSlotsDisponiblesAsync(
        int estacionId,
        DateTime fecha,
        int servicioId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = BuildSlotsKey(estacionId, fecha, servicioId);
            var cached = await db.StringGetAsync(key);

            if (cached.IsNull)
            {
                _logger.LogDebug(
                    "Cache MISS slots: est={EstacionId}, fecha={Fecha:yyyy-MM-dd}, serv={ServicioId}",
                    estacionId, fecha, servicioId);
                return null;
            }

            _logger.LogDebug(
                "Cache HIT slots: est={EstacionId}, fecha={Fecha:yyyy-MM-dd}, serv={ServicioId}",
                estacionId, fecha, servicioId);

            return JsonSerializer.Deserialize<List<SlotDisponibleDto>>(cached.ToString(), JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener slots del cache para estación {EstacionId}", estacionId);
            return null; // Fallback: retornar null para que se consulte la BD
        }
    }

    public async Task SetSlotsDisponiblesAsync(
        int estacionId,
        DateTime fecha,
        int servicioId,
        List<SlotDisponibleDto> slots,
        TimeSpan? expiration = null)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = BuildSlotsKey(estacionId, fecha, servicioId);
            var json = JsonSerializer.Serialize(slots, JsonOptions);

            await db.StringSetAsync(key, json, expiration ?? DefaultExpiration);

            _logger.LogDebug(
                "Cache SET slots: est={EstacionId}, fecha={Fecha:yyyy-MM-dd}, serv={ServicioId}, count={Count}",
                estacionId, fecha, servicioId, slots.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al guardar slots en cache para estación {EstacionId}", estacionId);
            // No lanzar excepción - el cache es opcional
        }
    }

    public async Task InvalidateSlotsAsync(int estacionId, DateTime fecha)
    {
        try
        {
            var db = _redis.GetDatabase();
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);

            // Patrón para eliminar todos los slots de esa estación y fecha (cualquier servicio)
            var pattern = $"{SlotsKeyPrefix}:est:{estacionId}:fecha:{fecha:yyyyMMdd}:*";

            var keysToDelete = new List<RedisKey>();
            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                keysToDelete.Add(key);
            }

            if (keysToDelete.Count > 0)
            {
                await db.KeyDeleteAsync(keysToDelete.ToArray());
                _logger.LogDebug(
                    "Cache INVALIDATE slots: est={EstacionId}, fecha={Fecha:yyyy-MM-dd}, keys={Count}",
                    estacionId, fecha, keysToDelete.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al invalidar cache de slots para estación {EstacionId}", estacionId);
        }
    }

    #endregion

    #region Reservas del Día

    public async Task<List<ReservaCacheDto>?> GetReservasDelDiaAsync(int estacionId, DateTime fecha)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = BuildReservasKey(estacionId, fecha);
            var cached = await db.StringGetAsync(key);

            if (cached.IsNull)
                return null;

            return JsonSerializer.Deserialize<List<ReservaCacheDto>>(cached.ToString(), JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener reservas del cache para estación {EstacionId}", estacionId);
            return null;
        }
    }

    public async Task SetReservasDelDiaAsync(
        int estacionId,
        DateTime fecha,
        List<ReservaCacheDto> reservas,
        TimeSpan? expiration = null)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = BuildReservasKey(estacionId, fecha);
            var json = JsonSerializer.Serialize(reservas, JsonOptions);

            await db.StringSetAsync(key, json, expiration ?? DefaultExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al guardar reservas en cache para estación {EstacionId}", estacionId);
        }
    }

    public async Task InvalidateReservasDelDiaAsync(int estacionId, DateTime fecha)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = BuildReservasKey(estacionId, fecha);
            await db.KeyDeleteAsync(key);

            _logger.LogDebug(
                "Cache INVALIDATE reservas: est={EstacionId}, fecha={Fecha:yyyy-MM-dd}",
                estacionId, fecha);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al invalidar cache de reservas para estación {EstacionId}", estacionId);
        }
    }

    #endregion

    #region Key Builders

    private static string BuildSlotsKey(int estacionId, DateTime fecha, int servicioId)
        => $"{SlotsKeyPrefix}:est:{estacionId}:fecha:{fecha:yyyyMMdd}:serv:{servicioId}";

    private static string BuildReservasKey(int estacionId, DateTime fecha)
        => $"{ReservasKeyPrefix}:est:{estacionId}:fecha:{fecha:yyyyMMdd}";

    #endregion
}


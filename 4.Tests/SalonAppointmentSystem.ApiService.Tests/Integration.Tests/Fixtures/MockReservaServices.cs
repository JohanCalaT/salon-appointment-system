using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;

namespace SalonAppointmentSystem.ApiService.Tests.Integration.Tests.Fixtures;

/// <summary>
/// Mock del servicio de locks para tests.
/// Siempre permite adquirir el lock inmediatamente.
/// </summary>
public class MockReservaLockService : IReservaLockService
{
    public Task<IAsyncDisposable?> AcquireLockAsync(
        int estacionId,
        DateTime fechaHora,
        int duracionMinutos,
        TimeSpan? waitTime = null,
        TimeSpan? lockTime = null)
    {
        // Siempre devuelve un lock exitoso
        return Task.FromResult<IAsyncDisposable?>(new MockLockHandle());
    }

    private sealed class MockLockHandle : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

/// <summary>
/// Mock del servicio de cache para tests.
/// No cachea nada, siempre devuelve null para forzar consultas a BD.
/// </summary>
public class MockReservaCacheService : IReservaCacheService
{
    public Task<List<SlotDisponibleDto>?> GetSlotsDisponiblesAsync(
        int estacionId, DateTime fecha, int servicioId)
        => Task.FromResult<List<SlotDisponibleDto>?>(null);

    public Task SetSlotsDisponiblesAsync(
        int estacionId, DateTime fecha, int servicioId,
        List<SlotDisponibleDto> slots, TimeSpan? expiration = null)
        => Task.CompletedTask;

    public Task InvalidateSlotsAsync(int estacionId, DateTime fecha)
        => Task.CompletedTask;

    public Task<List<ReservaCacheDto>?> GetReservasDelDiaAsync(int estacionId, DateTime fecha)
        => Task.FromResult<List<ReservaCacheDto>?>(null);

    public Task SetReservasDelDiaAsync(
        int estacionId, DateTime fecha,
        List<ReservaCacheDto> reservas, TimeSpan? expiration = null)
        => Task.CompletedTask;

    public Task InvalidateReservasDelDiaAsync(int estacionId, DateTime fecha)
        => Task.CompletedTask;
}


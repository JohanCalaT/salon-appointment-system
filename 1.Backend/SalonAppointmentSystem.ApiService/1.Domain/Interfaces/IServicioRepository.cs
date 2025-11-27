using SalonAppointmentSystem.ApiService.Domain.Entities;

namespace SalonAppointmentSystem.ApiService.Domain.Interfaces;

/// <summary>
/// Repositorio específico para Servicios
/// </summary>
public interface IServicioRepository : IRepository<Servicio>
{
    /// <summary>
    /// Obtiene todos los servicios activos
    /// </summary>
    Task<IReadOnlyList<Servicio>> GetActivosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe un servicio con el nombre dado
    /// </summary>
    Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene servicios ordenados por el campo Orden
    /// </summary>
    Task<IReadOnlyList<Servicio>> GetOrdenadosAsync(CancellationToken cancellationToken = default);
}


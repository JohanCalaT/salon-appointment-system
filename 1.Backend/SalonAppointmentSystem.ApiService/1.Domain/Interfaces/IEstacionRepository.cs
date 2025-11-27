using SalonAppointmentSystem.ApiService.Domain.Entities;

namespace SalonAppointmentSystem.ApiService.Domain.Interfaces;

/// <summary>
/// Repositorio específico para Estaciones
/// </summary>
public interface IEstacionRepository : IRepository<Estacion>
{
    /// <summary>
    /// Obtiene todas las estaciones activas
    /// </summary>
    Task<IReadOnlyList<Estacion>> GetActivasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la estación asignada a un barbero
    /// </summary>
    Task<Estacion?> GetByBarberoIdAsync(string barberoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe una estación con el nombre dado
    /// </summary>
    Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default);
}


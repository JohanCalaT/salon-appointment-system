using SalonAppointmentSystem.ApiService.Domain.Entities;

namespace SalonAppointmentSystem.ApiService.Domain.Interfaces;

/// <summary>
/// Repositorio específico para ConfiguracionGeneral
/// </summary>
public interface IConfiguracionGeneralRepository : IRepository<ConfiguracionGeneral>
{
    /// <summary>
    /// Obtiene una configuración por su clave
    /// </summary>
    Task<ConfiguracionGeneral?> GetByClaveAsync(
        string clave,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el valor de una configuración como entero
    /// </summary>
    Task<int> GetValorEnteroAsync(
        string clave,
        int defaultValue = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el valor de una configuración como booleano
    /// </summary>
    Task<bool> GetValorBooleanAsync(
        string clave,
        bool defaultValue = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el valor de una configuración como texto
    /// </summary>
    Task<string> GetValorTextoAsync(
        string clave,
        string defaultValue = "",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza o crea una configuración
    /// </summary>
    Task UpsertAsync(
        string clave,
        string valor,
        CancellationToken cancellationToken = default);
}


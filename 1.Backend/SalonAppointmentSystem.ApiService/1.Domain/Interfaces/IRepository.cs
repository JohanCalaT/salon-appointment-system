using System.Linq.Expressions;
using SalonAppointmentSystem.ApiService.Domain.Common;

namespace SalonAppointmentSystem.ApiService.Domain.Interfaces;

/// <summary>
/// Interface genérica para repositorios
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Obtiene una entidad por su ID
    /// </summary>
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las entidades
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca entidades según un predicado
    /// </summary>
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la primera entidad que cumple el predicado o null
    /// </summary>
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe alguna entidad que cumpla el predicado
    /// </summary>
    Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cuenta las entidades que cumplen el predicado
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega una nueva entidad
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega múltiples entidades
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una entidad existente
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Elimina una entidad
    /// </summary>
    void Delete(T entity);

    /// <summary>
    /// Elimina múltiples entidades
    /// </summary>
    void DeleteRange(IEnumerable<T> entities);
}


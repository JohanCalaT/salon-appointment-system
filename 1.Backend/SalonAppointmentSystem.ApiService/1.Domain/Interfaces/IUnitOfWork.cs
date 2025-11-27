namespace SalonAppointmentSystem.ApiService.Domain.Interfaces;

/// <summary>
/// Interface para el patrón Unit of Work
/// Coordina las operaciones de múltiples repositorios en una sola transacción
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Repositorio de Estaciones
    /// </summary>
    IEstacionRepository Estaciones { get; }

    /// <summary>
    /// Repositorio de Servicios
    /// </summary>
    IServicioRepository Servicios { get; }

    /// <summary>
    /// Repositorio de Reservas
    /// </summary>
    IReservaRepository Reservas { get; }

    /// <summary>
    /// Repositorio de Configuración de Horarios
    /// </summary>
    IConfiguracionHorarioRepository ConfiguracionHorarios { get; }

    /// <summary>
    /// Repositorio de Configuración General
    /// </summary>
    IConfiguracionGeneralRepository ConfiguracionGeneral { get; }

    /// <summary>
    /// Guarda todos los cambios pendientes en la base de datos
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia una transacción explícita
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma la transacción actual
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Revierte la transacción actual
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}


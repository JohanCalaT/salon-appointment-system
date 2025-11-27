using Microsoft.EntityFrameworkCore.Storage;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Repositories;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Persistence;

/// <summary>
/// Implementación del patrón Unit of Work
/// Coordina las operaciones de múltiples repositorios en una sola transacción
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // Repositorios (lazy initialization)
    private IEstacionRepository? _estaciones;
    private IServicioRepository? _servicios;
    private IReservaRepository? _reservas;
    private IConfiguracionHorarioRepository? _configuracionHorarios;
    private IConfiguracionGeneralRepository? _configuracionGeneral;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IEstacionRepository Estaciones =>
        _estaciones ??= new EstacionRepository(_context);

    public IServicioRepository Servicios =>
        _servicios ??= new ServicioRepository(_context);

    public IReservaRepository Reservas =>
        _reservas ??= new ReservaRepository(_context);

    public IConfiguracionHorarioRepository ConfiguracionHorarios =>
        _configuracionHorarios ??= new ConfiguracionHorarioRepository(_context);

    public IConfiguracionGeneralRepository ConfiguracionGeneral =>
        _configuracionGeneral ??= new ConfiguracionGeneralRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}


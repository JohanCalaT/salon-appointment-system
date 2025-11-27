using Microsoft.EntityFrameworkCore;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio específico para Estaciones
/// </summary>
public class EstacionRepository : Repository<Estacion>, IEstacionRepository
{
    public EstacionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Estacion>> GetActivasAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.Activa)
            .OrderBy(e => e.Orden)
            .ToListAsync(cancellationToken);
    }

    public async Task<Estacion?> GetByBarberoIdAsync(string barberoId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(e => e.BarberoId == barberoId, cancellationToken);
    }

    public async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(e => e.Nombre == nombre);
        
        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}


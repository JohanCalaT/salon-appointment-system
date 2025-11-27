using Microsoft.EntityFrameworkCore;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio específico para Servicios
/// </summary>
public class ServicioRepository : Repository<Servicio>, IServicioRepository
{
    public ServicioRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Servicio>> GetActivosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Activo)
            .OrderBy(s => s.Orden)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(s => s.Nombre == nombre);
        
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Servicio>> GetOrdenadosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderBy(s => s.Orden)
            .ThenBy(s => s.Nombre)
            .ToListAsync(cancellationToken);
    }
}


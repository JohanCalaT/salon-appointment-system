using Microsoft.EntityFrameworkCore;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio específico para Reservas
/// </summary>
public class ReservaRepository : Repository<Reserva>, IReservaRepository
{
    public ReservaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Reserva>> GetByEstacionYFechaAsync(
        int estacionId,
        DateTime fechaInicio,
        DateTime fechaFin,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Servicio)
            .Where(r => r.EstacionId == estacionId &&
                        r.FechaHora >= fechaInicio &&
                        r.FechaHora <= fechaFin &&
                        r.Estado != EstadoReserva.Cancelada)
            .OrderBy(r => r.FechaHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Reserva>> GetByUsuarioIdAsync(
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Estacion)
            .Include(r => r.Servicio)
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.FechaHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Reserva>> GetByEmailClienteAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Estacion)
            .Include(r => r.Servicio)
            .Where(r => r.EmailCliente == email)
            .OrderByDescending(r => r.FechaHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteSolapamientoAsync(
        int estacionId,
        DateTime fechaHora,
        int duracionMinutos,
        int? excludeReservaId = null,
        CancellationToken cancellationToken = default)
    {
        var fechaHoraFin = fechaHora.AddMinutes(duracionMinutos);

        var query = _dbSet
            .Where(r => r.EstacionId == estacionId &&
                        r.Estado != EstadoReserva.Cancelada &&
                        r.FechaHora < fechaHoraFin &&
                        r.FechaHora.AddMinutes(r.DuracionMinutos) > fechaHora);

        if (excludeReservaId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservaId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Reserva>> GetReservasFuturasAsync(
        int estacionId,
        DateTime desde,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Servicio)
            .Where(r => r.EstacionId == estacionId &&
                        r.FechaHora >= desde &&
                        r.Estado != EstadoReserva.Cancelada)
            .OrderBy(r => r.FechaHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Reserva>> GetByEstadoAsync(
        EstadoReserva estado,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Estacion)
            .Include(r => r.Servicio)
            .Where(r => r.Estado == estado)
            .OrderBy(r => r.FechaHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<Reserva?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Estacion)
            .Include(r => r.Servicio)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}


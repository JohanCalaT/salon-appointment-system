using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence;
using SalonAppointmentSystem.Shared.DTOs.Estaciones;
using SalonAppointmentSystem.Shared.Enums;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Services;

/// <summary>
/// Servicio para gestión de estaciones
/// </summary>
public class EstacionService : IEstacionService
{
    private readonly ApplicationDbContext _context;
    private readonly IEstacionRepository _estacionRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EstacionService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public EstacionService(
        ApplicationDbContext context,
        IEstacionRepository estacionRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<EstacionService> logger,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _estacionRepository = estacionRepository;
        _userManager = userManager;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    #region Lectura

    public async Task<ApiResponse<EstacionDto>> GetByIdAsync(int estacionId, CancellationToken cancellationToken = default)
    {
        var estacion = await _context.Estaciones
            .Include(e => e.Barbero)
            .FirstOrDefaultAsync(e => e.Id == estacionId, cancellationToken);

        if (estacion == null)
        {
            return ApiResponse<EstacionDto>.Fail("Estación no encontrada");
        }

        var dto = await MapToEstacionDtoAsync(estacion, cancellationToken);
        return ApiResponse<EstacionDto>.Ok(dto);
    }

    public async Task<ApiResponse<IReadOnlyList<EstacionDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var estaciones = await _context.Estaciones
            .Include(e => e.Barbero)
            .OrderBy(e => e.Orden)
            .ThenBy(e => e.Nombre)
            .ToListAsync(cancellationToken);

        var dtos = new List<EstacionDto>();
        foreach (var estacion in estaciones)
        {
            dtos.Add(await MapToEstacionDtoAsync(estacion, cancellationToken));
        }

        return ApiResponse<IReadOnlyList<EstacionDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<PagedResult<EstacionDto>>> GetPagedAsync(EstacionListFilters filters, CancellationToken cancellationToken = default)
    {
        var query = _context.Estaciones
            .Include(e => e.Barbero)
            .AsQueryable();

        // Aplicar filtros
        query = ApplyFilters(query, filters);

        // Contar total
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar ordenamiento
        query = ApplyOrdering(query, filters);

        // Aplicar paginación
        var estaciones = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<EstacionDto>();
        foreach (var estacion in estaciones)
        {
            dtos.Add(await MapToEstacionDtoAsync(estacion, cancellationToken));
        }

        var result = PagedResult<EstacionDto>.Create(dtos, totalCount, filters.PageNumber, filters.PageSize);
        return ApiResponse<PagedResult<EstacionDto>>.Ok(result);
    }

    public async Task<ApiResponse<EstacionDto>> GetByBarberoIdAsync(string barberoId, CancellationToken cancellationToken = default)
    {
        var estacion = await _context.Estaciones
            .Include(e => e.Barbero)
            .FirstOrDefaultAsync(e => e.BarberoId == barberoId, cancellationToken);

        if (estacion == null)
        {
            return ApiResponse<EstacionDto>.Fail("El barbero no tiene estación asignada");
        }

        var dto = await MapToEstacionDtoAsync(estacion, cancellationToken);
        return ApiResponse<EstacionDto>.Ok(dto);
    }

    public async Task<ApiResponse<IReadOnlyList<EstacionDto>>> GetActivasParaReservasAsync(CancellationToken cancellationToken = default)
    {
        var estaciones = await _context.Estaciones
            .Include(e => e.Barbero)
            .Where(e => e.Activa && e.BarberoId != null)
            .OrderBy(e => e.Orden)
            .ToListAsync(cancellationToken);

        var dtos = new List<EstacionDto>();
        foreach (var estacion in estaciones)
        {
            dtos.Add(await MapToEstacionDtoAsync(estacion, cancellationToken));
        }

        return ApiResponse<IReadOnlyList<EstacionDto>>.Ok(dtos);
    }

    #endregion

    #region Escritura

    public async Task<ApiResponse<EstacionDto>> CreateAsync(CreateEstacionRequest request, CancellationToken cancellationToken = default)
    {
        // Validar nombre único
        if (await NombreExistsAsync(request.Nombre, null, cancellationToken))
        {
            return ApiResponse<EstacionDto>.Fail("Ya existe una estación con ese nombre");
        }

        // Validar barbero si se especifica
        if (!string.IsNullOrEmpty(request.BarberoId))
        {
            var validacionBarbero = await ValidarBarberoAsync(request.BarberoId, null, cancellationToken);
            if (!validacionBarbero.Success)
            {
                return ApiResponse<EstacionDto>.Fail(validacionBarbero.Message);
            }
        }

        var estacion = new Estacion
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            BarberoId = request.BarberoId,
            Activa = request.Activa,
            Orden = request.Orden,
            UsaHorarioGenerico = request.UsaHorarioGenerico
        };

        await _estacionRepository.AddAsync(estacion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Estación creada: {Nombre} (ID: {Id})", estacion.Nombre, estacion.Id);

        // Recargar con navegación
        var estacionCreada = await _context.Estaciones
            .Include(e => e.Barbero)
            .FirstAsync(e => e.Id == estacion.Id, cancellationToken);

        var dto = await MapToEstacionDtoAsync(estacionCreada, cancellationToken);
        return ApiResponse<EstacionDto>.Ok(dto, "Estación creada correctamente");
    }

    public async Task<ApiResponse<EstacionDto>> UpdateAsync(int estacionId, UpdateEstacionRequest request, CancellationToken cancellationToken = default)
    {
        var estacion = await _context.Estaciones
            .Include(e => e.Barbero)
            .FirstOrDefaultAsync(e => e.Id == estacionId, cancellationToken);

        if (estacion == null)
        {
            return ApiResponse<EstacionDto>.Fail("Estación no encontrada");
        }

        // Validar nombre único
        if (await NombreExistsAsync(request.Nombre, estacionId, cancellationToken))
        {
            return ApiResponse<EstacionDto>.Fail("Ya existe otra estación con ese nombre");
        }

        // Validar barbero si se especifica
        if (!string.IsNullOrEmpty(request.BarberoId))
        {
            var validacionBarbero = await ValidarBarberoAsync(request.BarberoId, estacionId, cancellationToken);
            if (!validacionBarbero.Success)
            {
                return ApiResponse<EstacionDto>.Fail(validacionBarbero.Message);
            }
        }

        estacion.Nombre = request.Nombre;
        estacion.Descripcion = request.Descripcion;
        estacion.BarberoId = request.BarberoId;
        estacion.Activa = request.Activa;
        estacion.Orden = request.Orden;
        estacion.UsaHorarioGenerico = request.UsaHorarioGenerico;

        _estacionRepository.Update(estacion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Estación actualizada: {Nombre} (ID: {Id})", estacion.Nombre, estacion.Id);

        var dto = await MapToEstacionDtoAsync(estacion, cancellationToken);
        return ApiResponse<EstacionDto>.Ok(dto, "Estación actualizada correctamente");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int estacionId, CancellationToken cancellationToken = default)
    {
        var estacion = await _estacionRepository.GetByIdAsync(estacionId, cancellationToken);
        if (estacion == null)
        {
            return ApiResponse<bool>.Fail("Estación no encontrada");
        }

        // Verificar si tiene reservas activas
        var tieneReservas = await _context.Reservas
            .AnyAsync(r => r.EstacionId == estacionId &&
                          r.Estado != EstadoReserva.Cancelada &&
                          r.FechaHora >= DateTime.UtcNow, cancellationToken);

        if (tieneReservas)
        {
            return ApiResponse<bool>.Fail("No se puede eliminar la estación porque tiene reservas pendientes");
        }

        // Soft delete
        estacion.Activa = false;
        _estacionRepository.Update(estacion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Estación eliminada (soft delete): {Nombre} (ID: {Id})", estacion.Nombre, estacion.Id);
        return ApiResponse<bool>.Ok(true, "Estación eliminada correctamente");
    }

    public async Task<ApiResponse<bool>> ToggleActiveAsync(int estacionId, CancellationToken cancellationToken = default)
    {
        var estacion = await _estacionRepository.GetByIdAsync(estacionId, cancellationToken);
        if (estacion == null)
        {
            return ApiResponse<bool>.Fail("Estación no encontrada");
        }

        estacion.Activa = !estacion.Activa;
        _estacionRepository.Update(estacion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var estado = estacion.Activa ? "activada" : "desactivada";
        _logger.LogInformation("Estación {Estado}: {Nombre} (ID: {Id})", estado, estacion.Nombre, estacion.Id);
        return ApiResponse<bool>.Ok(estacion.Activa, $"Estación {estado} correctamente");
    }

    public async Task<ApiResponse<EstacionDto>> AsignarBarberoAsync(int estacionId, AsignarBarberoRequest request, CancellationToken cancellationToken = default)
    {
        var estacion = await _context.Estaciones
            .Include(e => e.Barbero)
            .FirstOrDefaultAsync(e => e.Id == estacionId, cancellationToken);

        if (estacion == null)
        {
            return ApiResponse<EstacionDto>.Fail("Estación no encontrada");
        }

        // Validar barbero si se especifica
        if (!string.IsNullOrEmpty(request.BarberoId))
        {
            var validacionBarbero = await ValidarBarberoAsync(request.BarberoId, estacionId, cancellationToken);
            if (!validacionBarbero.Success)
            {
                return ApiResponse<EstacionDto>.Fail(validacionBarbero.Message);
            }
        }

        var barberoAnterior = estacion.BarberoId;
        estacion.BarberoId = request.BarberoId;

        _estacionRepository.Update(estacion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recargar para obtener el nuevo barbero
        await _context.Entry(estacion).Reference(e => e.Barbero).LoadAsync(cancellationToken);

        var accion = string.IsNullOrEmpty(request.BarberoId) ? "desasignado" : "asignado";
        _logger.LogInformation("Barbero {Accion} en estación {Nombre}: {BarberoAnterior} -> {BarberoNuevo}",
            accion, estacion.Nombre, barberoAnterior, request.BarberoId);

        var dto = await MapToEstacionDtoAsync(estacion, cancellationToken);
        return ApiResponse<EstacionDto>.Ok(dto, $"Barbero {accion} correctamente");
    }

    #endregion

    #region Validaciones

    public async Task<bool> NombreExistsAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Estaciones.Where(e => e.Nombre == nombre);
        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> BarberoYaAsignadoAsync(string barberoId, int? excludeEstacionId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Estaciones.Where(e => e.BarberoId == barberoId);
        if (excludeEstacionId.HasValue)
        {
            query = query.Where(e => e.Id != excludeEstacionId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }

    private async Task<ApiResponse<bool>> ValidarBarberoAsync(string barberoId, int? excludeEstacionId, CancellationToken cancellationToken)
    {
        var barbero = await _userManager.FindByIdAsync(barberoId);
        if (barbero == null)
        {
            return ApiResponse<bool>.Fail("El barbero especificado no existe");
        }

        if (!barbero.Activo)
        {
            return ApiResponse<bool>.Fail("El barbero especificado no está activo");
        }

        var esBarbero = await _userManager.IsInRoleAsync(barbero, ApplicationRoles.Barbero);
        if (!esBarbero)
        {
            return ApiResponse<bool>.Fail("El usuario especificado no tiene rol de barbero");
        }

        if (await BarberoYaAsignadoAsync(barberoId, excludeEstacionId, cancellationToken))
        {
            return ApiResponse<bool>.Fail("El barbero ya está asignado a otra estación");
        }

        return ApiResponse<bool>.Ok(true);
    }

    #endregion

    #region Helpers

    private IQueryable<Estacion> ApplyFilters(IQueryable<Estacion> query, EstacionListFilters filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Nombre))
        {
            query = query.Where(e => e.Nombre.Contains(filters.Nombre));
        }

        if (filters.Activa.HasValue)
        {
            query = query.Where(e => e.Activa == filters.Activa.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.BarberoId))
        {
            query = query.Where(e => e.BarberoId == filters.BarberoId);
        }

        if (filters.SinBarbero == true)
        {
            query = query.Where(e => e.BarberoId == null);
        }

        if (filters.UsaHorarioGenerico.HasValue)
        {
            query = query.Where(e => e.UsaHorarioGenerico == filters.UsaHorarioGenerico.Value);
        }

        return query;
    }

    private static IQueryable<Estacion> ApplyOrdering(IQueryable<Estacion> query, EstacionListFilters filters)
    {
        return filters.OrderBy?.ToLower() switch
        {
            "nombre" => filters.OrderDescending
                ? query.OrderByDescending(e => e.Nombre)
                : query.OrderBy(e => e.Nombre),
            "activa" => filters.OrderDescending
                ? query.OrderByDescending(e => e.Activa)
                : query.OrderBy(e => e.Activa),
            _ => filters.OrderDescending
                ? query.OrderByDescending(e => e.Orden).ThenByDescending(e => e.Nombre)
                : query.OrderBy(e => e.Orden).ThenBy(e => e.Nombre)
        };
    }

    private async Task<EstacionDto> MapToEstacionDtoAsync(Estacion estacion, CancellationToken cancellationToken)
    {
        var reservasHoy = await _context.Reservas
            .CountAsync(r => r.EstacionId == estacion.Id &&
                            r.FechaHora.Date == DateTime.UtcNow.Date &&
                            r.Estado != EstadoReserva.Cancelada, cancellationToken);

        return new EstacionDto
        {
            Id = estacion.Id,
            Nombre = estacion.Nombre,
            Descripcion = estacion.Descripcion,
            BarberoId = estacion.BarberoId,
            NombreBarbero = estacion.Barbero?.NombreCompleto,
            EmailBarbero = estacion.Barbero?.Email,
            Activa = estacion.Activa,
            Orden = estacion.Orden,
            UsaHorarioGenerico = estacion.UsaHorarioGenerico,
            PuedeRecibirReservas = estacion.PuedeRecibirReservas,
            ReservasHoy = reservasHoy
        };
    }

    #endregion
}


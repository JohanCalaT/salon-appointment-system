using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence;
using SalonAppointmentSystem.Shared.DTOs.Servicios;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Services;

/// <summary>
/// Servicio para gestión de servicios de barbería
/// </summary>
public class ServicioService : IServicioService
{
    private readonly ApplicationDbContext _context;
    private readonly IServicioRepository _servicioRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ServicioService> _logger;

    public ServicioService(
        ApplicationDbContext context,
        IServicioRepository servicioRepository,
        IMapper mapper,
        ILogger<ServicioService> logger)
    {
        _context = context;
        _servicioRepository = servicioRepository;
        _mapper = mapper;
        _logger = logger;
    }

    #region Lectura

    public async Task<ApiResponse<ServicioDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var servicio = await _context.Servicios.FindAsync(new object[] { id }, cancellationToken);

        if (servicio == null)
        {
            return ApiResponse<ServicioDto>.Fail("Servicio no encontrado");
        }

        var dto = _mapper.Map<ServicioDto>(servicio);
        return ApiResponse<ServicioDto>.Ok(dto);
    }

    public async Task<ApiResponse<IReadOnlyList<ServicioDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var servicios = await _context.Servicios
            .OrderBy(s => s.Orden)
            .ThenBy(s => s.Nombre)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<ServicioDto>>(servicios);
        return ApiResponse<IReadOnlyList<ServicioDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<PagedResult<ServicioDto>>> GetPagedAsync(
        ServicioListFilters filters, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Servicios.AsQueryable();

        // Aplicar filtros
        query = ApplyFilters(query, filters);

        // Contar total antes de paginar
        var totalItems = await query.CountAsync(cancellationToken);

        // Aplicar ordenamiento
        query = ApplyOrdering(query, filters);

        // Paginar
        var servicios = await query
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<ServicioDto>>(servicios);

        var result = PagedResult<ServicioDto>.Create(
            dtos,
            totalItems,
            filters.Page,
            filters.PageSize
        );

        return ApiResponse<PagedResult<ServicioDto>>.Ok(result);
    }

    public async Task<ApiResponse<IReadOnlyList<ServicioDto>>> GetActivosAsync(
        CancellationToken cancellationToken = default)
    {
        var servicios = await _servicioRepository.GetActivosAsync(cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<ServicioDto>>(servicios);
        return ApiResponse<IReadOnlyList<ServicioDto>>.Ok(dtos);
    }

    #endregion

    #region Escritura

    public async Task<ApiResponse<ServicioDto>> CreateAsync(
        CreateServicioRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Validar nombre único
        if (await _servicioRepository.ExisteNombreAsync(request.Nombre, null, cancellationToken))
        {
            return ApiResponse<ServicioDto>.Fail("Ya existe un servicio con ese nombre");
        }

        var servicio = _mapper.Map<Servicio>(request);

        await _servicioRepository.AddAsync(servicio, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Servicio creado: {Nombre} (ID: {Id})", servicio.Nombre, servicio.Id);

        var dto = _mapper.Map<ServicioDto>(servicio);
        return ApiResponse<ServicioDto>.Ok(dto, "Servicio creado exitosamente");
    }

    public async Task<ApiResponse<ServicioDto>> UpdateAsync(
        int id, 
        UpdateServicioRequest request, 
        CancellationToken cancellationToken = default)
    {
        var servicio = await _context.Servicios.FindAsync(new object[] { id }, cancellationToken);

        if (servicio == null)
        {
            return ApiResponse<ServicioDto>.Fail("Servicio no encontrado");
        }

        // Validar nombre único (excluyendo el actual)
        if (await _servicioRepository.ExisteNombreAsync(request.Nombre, id, cancellationToken))
        {
            return ApiResponse<ServicioDto>.Fail("Ya existe otro servicio con ese nombre");
        }

        // Actualizar propiedades
        _mapper.Map(request, servicio);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Servicio actualizado: {Nombre} (ID: {Id})", servicio.Nombre, servicio.Id);

        var dto = _mapper.Map<ServicioDto>(servicio);
        return ApiResponse<ServicioDto>.Ok(dto, "Servicio actualizado exitosamente");
    }

    public async Task<ApiResponse<ServicioDto>> PatchAsync(
        int id,
        PatchServicioRequest request,
        CancellationToken cancellationToken = default)
    {
        var servicio = await _context.Servicios.FindAsync(new object[] { id }, cancellationToken);

        if (servicio == null)
        {
            return ApiResponse<ServicioDto>.Fail("Servicio no encontrado");
        }

        // Validar nombre único si se proporciona
        if (!string.IsNullOrEmpty(request.Nombre) &&
            await _servicioRepository.ExisteNombreAsync(request.Nombre, id, cancellationToken))
        {
            return ApiResponse<ServicioDto>.Fail("Ya existe otro servicio con ese nombre");
        }

        // Actualizar solo los campos proporcionados
        if (request.Nombre != null)
            servicio.Nombre = request.Nombre;

        if (request.Descripcion != null)
            servicio.Descripcion = request.Descripcion;

        if (request.DuracionMinutos.HasValue)
            servicio.DuracionMinutos = request.DuracionMinutos.Value;

        if (request.Precio.HasValue)
            servicio.Precio = request.Precio.Value;

        if (request.PuntosQueOtorga.HasValue)
            servicio.PuntosQueOtorga = request.PuntosQueOtorga.Value;

        if (request.Activo.HasValue)
            servicio.Activo = request.Activo.Value;

        if (request.Orden.HasValue)
            servicio.Orden = request.Orden.Value;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Servicio actualizado parcialmente: {Nombre} (ID: {Id})", servicio.Nombre, servicio.Id);

        var dto = _mapper.Map<ServicioDto>(servicio);
        return ApiResponse<ServicioDto>.Ok(dto, "Servicio actualizado exitosamente");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var servicio = await _context.Servicios
            .Include(s => s.Reservas)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (servicio == null)
        {
            return ApiResponse<bool>.Fail("Servicio no encontrado");
        }

        // Verificar si tiene reservas activas
        var tieneReservasActivas = servicio.Reservas.Any(r =>
            r.Estado != Shared.Enums.EstadoReserva.Cancelada &&
            r.FechaHora >= DateTime.UtcNow);

        if (tieneReservasActivas)
        {
            return ApiResponse<bool>.Fail("No se puede eliminar el servicio porque tiene reservas pendientes");
        }

        // Soft delete: desactivar en lugar de eliminar
        servicio.Activo = false;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Servicio eliminado (soft delete): {Nombre} (ID: {Id})", servicio.Nombre, servicio.Id);

        return ApiResponse<bool>.Ok(true, "Servicio eliminado exitosamente");
    }

    public async Task<ApiResponse<bool>> ToggleActivoAsync(int id, CancellationToken cancellationToken = default)
    {
        var servicio = await _context.Servicios.FindAsync(new object[] { id }, cancellationToken);

        if (servicio == null)
        {
            return ApiResponse<bool>.Fail("Servicio no encontrado");
        }

        servicio.Activo = !servicio.Activo;
        await _context.SaveChangesAsync(cancellationToken);

        var estado = servicio.Activo ? "activado" : "desactivado";
        _logger.LogInformation("Servicio {Estado}: {Nombre} (ID: {Id})", estado, servicio.Nombre, servicio.Id);

        return ApiResponse<bool>.Ok(true, $"Servicio {estado} exitosamente");
    }

    #endregion

    #region Validaciones

    public async Task<bool> NombreExistsAsync(
        string nombre,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return await _servicioRepository.ExisteNombreAsync(nombre, excludeId, cancellationToken);
    }

    #endregion

    #region Métodos Privados

    private static IQueryable<Servicio> ApplyFilters(IQueryable<Servicio> query, ServicioListFilters filters)
    {
        // Búsqueda por nombre o descripción
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var searchLower = filters.Search.ToLower();
            query = query.Where(s =>
                s.Nombre.ToLower().Contains(searchLower) ||
                (s.Descripcion != null && s.Descripcion.ToLower().Contains(searchLower)));
        }

        // Filtrar por activo
        if (filters.Activo.HasValue)
        {
            query = query.Where(s => s.Activo == filters.Activo.Value);
        }

        // Filtrar por precio mínimo
        if (filters.PrecioMin.HasValue)
        {
            query = query.Where(s => s.Precio >= filters.PrecioMin.Value);
        }

        // Filtrar por precio máximo
        if (filters.PrecioMax.HasValue)
        {
            query = query.Where(s => s.Precio <= filters.PrecioMax.Value);
        }

        return query;
    }

    private static IQueryable<Servicio> ApplyOrdering(IQueryable<Servicio> query, ServicioListFilters filters)
    {
        var isDescending = filters.OrderDirection?.ToLower() == "desc";

        return filters.OrderBy?.ToLower() switch
        {
            "nombre" => isDescending
                ? query.OrderByDescending(s => s.Nombre)
                : query.OrderBy(s => s.Nombre),
            "precio" => isDescending
                ? query.OrderByDescending(s => s.Precio)
                : query.OrderBy(s => s.Precio),
            "duracion" => isDescending
                ? query.OrderByDescending(s => s.DuracionMinutos)
                : query.OrderBy(s => s.DuracionMinutos),
            _ => isDescending
                ? query.OrderByDescending(s => s.Orden).ThenByDescending(s => s.Nombre)
                : query.OrderBy(s => s.Orden).ThenBy(s => s.Nombre)
        };
    }

    #endregion
}


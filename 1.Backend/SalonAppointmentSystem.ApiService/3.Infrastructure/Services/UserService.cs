using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.ApiService.Domain.Interfaces;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence;
using SalonAppointmentSystem.Shared.DTOs.Users;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Services;

/// <summary>
/// Servicio para gestión de usuarios
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    // Roles válidos para asignación
    private static readonly string[] ValidRoles = { ApplicationRoles.Admin, ApplicationRoles.Barbero, ApplicationRoles.Cliente };

    public UserService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<UserService> logger,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    #region Lectura

    public async Task<ApiResponse<UserDto>> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Estacion)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return ApiResponse<UserDto>.Fail("Usuario no encontrado");
        }

        var userDto = await MapToUserDtoAsync(user);
        return ApiResponse<UserDto>.Ok(userDto);
    }

    public async Task<ApiResponse<IReadOnlyList<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .Include(u => u.Estacion)
            .OrderByDescending(u => u.FechaRegistro)
            .ToListAsync(cancellationToken);

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            userDtos.Add(await MapToUserDtoAsync(user));
        }

        return ApiResponse<IReadOnlyList<UserDto>>.Ok(userDtos);
    }

    public async Task<ApiResponse<PagedResult<UserDto>>> GetPagedAsync(UserListFilters filters, CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .Include(u => u.Estacion)
            .AsQueryable();

        // Aplicar filtros básicos
        query = ApplyFilters(query, filters);

        // Aplicar filtro por rol si se especifica
        if (!string.IsNullOrWhiteSpace(filters.Rol))
        {
            // Obtener IDs de usuarios con el rol especificado
            var usersInRole = await _userManager.GetUsersInRoleAsync(filters.Rol);
            var userIdsInRole = usersInRole.Select(u => u.Id).ToHashSet();
            query = query.Where(u => userIdsInRole.Contains(u.Id));
        }

        // Contar total antes de paginación
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar ordenamiento
        query = ApplyOrdering(query, filters);

        // Aplicar paginación
        var users = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync(cancellationToken);

        // Mapear a DTOs
        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            userDtos.Add(await MapToUserDtoAsync(user));
        }

        var pagedResult = PagedResult<UserDto>.Create(
            userDtos,
            totalCount,
            filters.PageNumber,
            filters.PageSize);

        return ApiResponse<PagedResult<UserDto>>.Ok(pagedResult);
    }

    #endregion

    #region Escritura

    public async Task<ApiResponse<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        // Validar rol
        if (!ValidRoles.Contains(request.Rol))
        {
            return ApiResponse<UserDto>.Fail($"Rol inválido. Roles válidos: {string.Join(", ", ValidRoles)}");
        }

        // Validar email único
        if (await EmailExistsAsync(request.Email, null, cancellationToken))
        {
            return ApiResponse<UserDto>.Fail("El email ya está registrado");
        }

        // Validar estación para barberos
        if (request.Rol == ApplicationRoles.Barbero && request.EstacionId.HasValue)
        {
            var estacionExiste = await _unitOfWork.Estaciones.AnyAsync(e => e.Id == request.EstacionId.Value, cancellationToken);
            if (!estacionExiste)
            {
                return ApiResponse<UserDto>.Fail("La estación especificada no existe");
            }
        }

        // Crear usuario
        var user = _mapper.Map<ApplicationUser>(request);

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Error al crear usuario {Email}: {Errors}", request.Email, string.Join(", ", errors));
            return ApiResponse<UserDto>.Fail("Error al crear usuario", errors);
        }

        // Asignar rol
        await _userManager.AddToRoleAsync(user, request.Rol);
        _logger.LogInformation("Usuario creado: {Email} con rol {Rol}", request.Email, request.Rol);

        var userDto = await MapToUserDtoAsync(user);
        return ApiResponse<UserDto>.Ok(userDto, "Usuario creado exitosamente");
    }

    public async Task<ApiResponse<UserDto>> UpdateAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Estacion)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return ApiResponse<UserDto>.Fail("Usuario no encontrado");
        }

        // Validar rol
        if (!ValidRoles.Contains(request.Rol))
        {
            return ApiResponse<UserDto>.Fail($"Rol inválido. Roles válidos: {string.Join(", ", ValidRoles)}");
        }

        // Validar email único (excluyendo el usuario actual)
        if (await EmailExistsAsync(request.Email, userId, cancellationToken))
        {
            return ApiResponse<UserDto>.Fail("El email ya está registrado por otro usuario");
        }

        // Validar estación para barberos
        if (request.Rol == ApplicationRoles.Barbero && request.EstacionId.HasValue)
        {
            var estacionExiste = await _unitOfWork.Estaciones.AnyAsync(e => e.Id == request.EstacionId.Value, cancellationToken);
            if (!estacionExiste)
            {
                return ApiResponse<UserDto>.Fail("La estación especificada no existe");
            }
        }

        // Actualizar propiedades
        _mapper.Map(request, user);

        // Actualizar rol si cambió
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(request.Rol))
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, request.Rol);
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<UserDto>.Fail("Error al actualizar usuario", errors);
        }

        _logger.LogInformation("Usuario actualizado: {UserId}", userId);
        var userDto = await MapToUserDtoAsync(user);
        return ApiResponse<UserDto>.Ok(userDto, "Usuario actualizado exitosamente");
    }

    public async Task<ApiResponse<UserDto>> PatchAsync(string userId, PatchUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Estacion)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return ApiResponse<UserDto>.Fail("Usuario no encontrado");
        }

        // Aplicar solo los campos que vienen en el request
        if (!string.IsNullOrEmpty(request.Email))
        {
            if (await EmailExistsAsync(request.Email, userId, cancellationToken))
            {
                return ApiResponse<UserDto>.Fail("El email ya está registrado por otro usuario");
            }
            user.Email = request.Email;
            user.UserName = request.Email;
        }

        if (!string.IsNullOrEmpty(request.NombreCompleto))
            user.NombreCompleto = request.NombreCompleto;

        if (request.PhoneNumber != null)
            user.PhoneNumber = request.PhoneNumber;

        if (request.FotoUrl != null)
            user.FotoUrl = request.FotoUrl;

        if (request.Activo.HasValue)
            user.Activo = request.Activo.Value;

        if (request.PuntosAcumulados.HasValue)
            user.PuntosAcumulados = request.PuntosAcumulados.Value;

        // Manejar EstacionId (puede ser null explícito)
        if (request.EstacionIdSpecified)
        {
            if (request.EstacionId.HasValue)
            {
                var estacionExiste = await _unitOfWork.Estaciones.AnyAsync(e => e.Id == request.EstacionId.Value, cancellationToken);
                if (!estacionExiste)
                {
                    return ApiResponse<UserDto>.Fail("La estación especificada no existe");
                }
            }
            user.EstacionId = request.EstacionId;
        }

        // Cambiar rol si se especifica
        if (!string.IsNullOrEmpty(request.Rol))
        {
            if (!ValidRoles.Contains(request.Rol))
            {
                return ApiResponse<UserDto>.Fail($"Rol inválido. Roles válidos: {string.Join(", ", ValidRoles)}");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(request.Rol))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, request.Rol);
            }
        }

        // Cambiar contraseña si se especifica
        if (!string.IsNullOrEmpty(request.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!passwordResult.Succeeded)
            {
                var errors = passwordResult.Errors.Select(e => e.Description).ToList();
                return ApiResponse<UserDto>.Fail("Error al cambiar contraseña", errors);
            }
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<UserDto>.Fail("Error al actualizar usuario", errors);
        }

        _logger.LogInformation("Usuario actualizado parcialmente: {UserId}", userId);
        var userDto = await MapToUserDtoAsync(user);
        return ApiResponse<UserDto>.Ok(userDto, "Usuario actualizado exitosamente");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<bool>.Fail("Usuario no encontrado");
        }

        // Soft delete - solo desactivar
        user.Activo = false;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<bool>.Fail("Error al eliminar usuario", errors);
        }

        _logger.LogInformation("Usuario eliminado (soft delete): {UserId}", userId);
        return ApiResponse<bool>.Ok(true, "Usuario eliminado exitosamente");
    }

    #endregion

    #region Acciones Especiales

    public async Task<ApiResponse<bool>> ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<bool>.Fail("Usuario no encontrado");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<bool>.Fail("Error al cambiar contraseña", errors);
        }

        _logger.LogInformation("Contraseña reseteada para usuario: {UserId}", userId);
        return ApiResponse<bool>.Ok(true, "Contraseña actualizada exitosamente");
    }

    public async Task<ApiResponse<bool>> ToggleActiveAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<bool>.Fail("Usuario no encontrado");
        }

        user.Activo = !user.Activo;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<bool>.Fail("Error al cambiar estado del usuario", errors);
        }

        var estado = user.Activo ? "activado" : "desactivado";
        _logger.LogInformation("Usuario {Estado}: {UserId}", estado, userId);
        return ApiResponse<bool>.Ok(user.Activo, $"Usuario {estado} exitosamente");
    }

    public async Task<bool> EmailExistsAsync(string email, string? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.Where(u => u.Email == email);

        if (!string.IsNullOrEmpty(excludeUserId))
        {
            query = query.Where(u => u.Id != excludeUserId);
        }

        return await query.AnyAsync(cancellationToken);
    }

    #endregion

    #region Private Methods

    private async Task<UserDto> MapToUserDtoAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var userDto = _mapper.Map<UserDto>(user);

        // Asignar rol manualmente (no se puede mapear automáticamente)
        return userDto with { Rol = roles.FirstOrDefault() ?? string.Empty };
    }

    private static IQueryable<ApplicationUser> ApplyFilters(IQueryable<ApplicationUser> query, UserListFilters filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var searchLower = filters.Search.ToLower();
            query = query.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(searchLower)) ||
                u.NombreCompleto.ToLower().Contains(searchLower));
        }

        if (filters.Activo.HasValue)
        {
            query = query.Where(u => u.Activo == filters.Activo.Value);
        }

        if (filters.EstacionId.HasValue)
        {
            query = query.Where(u => u.EstacionId == filters.EstacionId.Value);
        }

        // Nota: El filtro por rol se aplica después del query porque requiere UserManager
        return query;
    }

    private static IQueryable<ApplicationUser> ApplyOrdering(IQueryable<ApplicationUser> query, UserListFilters filters)
    {
        query = filters.OrderBy?.ToLower() switch
        {
            "email" => filters.Descending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            "nombrecompleto" => filters.Descending
                ? query.OrderByDescending(u => u.NombreCompleto)
                : query.OrderBy(u => u.NombreCompleto),
            "puntosacumulados" => filters.Descending
                ? query.OrderByDescending(u => u.PuntosAcumulados)
                : query.OrderBy(u => u.PuntosAcumulados),
            _ => filters.Descending
                ? query.OrderByDescending(u => u.FechaRegistro)
                : query.OrderBy(u => u.FechaRegistro)
        };

        return query;
    }

    #endregion
}


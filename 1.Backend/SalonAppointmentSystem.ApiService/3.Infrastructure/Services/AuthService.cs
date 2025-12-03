using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SalonAppointmentSystem.ApiService.Application.Common.Constants;
using SalonAppointmentSystem.ApiService.Application.Common.Interfaces;
using SalonAppointmentSystem.ApiService.Application.Common.Settings;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.Shared.DTOs.Auth;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence;

namespace SalonAppointmentSystem.ApiService.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de autenticación con JWT
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
                return AuthResponse.Fail("Credenciales inválidas");
            }

            if (!user.Activo)
            {
                _logger.LogWarning("Login failed: User {Email} is inactive", request.Email);
                return AuthResponse.Fail("Usuario inactivo. Contacte al administrador.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Login failed: Invalid password for {Email}", request.Email);
                return AuthResponse.Fail("Credenciales inválidas");
            }

            return await GenerateAuthResponseAsync(user, ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return AuthResponse.Fail("Error interno durante la autenticación");
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress = null)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return AuthResponse.Fail("El email ya está registrado");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                NombreCompleto = request.NombreCompleto,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = true,
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Registration failed for {Email}: {Errors}", request.Email, errors);
                return AuthResponse.Fail($"Error al registrar: {errors}");
            }

            // Asignar rol de Cliente por defecto
            await _userManager.AddToRoleAsync(user, ApplicationRoles.Cliente);
            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return await GenerateAuthResponseAsync(user, ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", request.Email);
            return AuthResponse.Fail("Error interno durante el registro");
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null)
    {
        try
        {
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
            {
                return AuthResponse.Fail("Token de acceso inválido");
            }

            var userId = principal.FindFirst(AppClaimTypes.UserId)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return AuthResponse.Fail("Token de acceso inválido");
            }

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && t.UserId == userId);

            if (storedToken == null || !storedToken.IsActive)
            {
                return AuthResponse.Fail("Refresh token inválido o expirado");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.Activo)
            {
                return AuthResponse.Fail("Usuario no encontrado o inactivo");
            }

            // Revocar el token actual
            storedToken.Revoke("Replaced by new token", ipAddress);

            // Generar nueva respuesta con nuevos tokens
            var response = await GenerateAuthResponseAsync(user, ipAddress);

            // Actualizar referencia al nuevo token
            storedToken.ReplacedByToken = response.RefreshToken;
            await _context.SaveChangesAsync();

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return AuthResponse.Fail("Error interno durante la renovación del token");
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null || !token.IsActive)
            return false;

        token.Revoke("Revoked by user logout", ipAddress);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Refresh token revoked for user {UserId}", token.UserId);
        return true;
    }

    public async Task<int> RevokeAllUserTokensAsync(string userId, string? ipAddress = null)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.Revoke("Revoked all tokens", ipAddress);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Revoked {Count} tokens for user {UserId}", activeTokens.Count, userId);

        return activeTokens.Count;
    }

    #region Private Methods

    private async Task<AuthResponse> GenerateAuthResponseAsync(ApplicationUser user, string? ipAddress)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? ApplicationRoles.Cliente;

        var accessToken = GenerateAccessToken(user, primaryRole);
        var refreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id, ipAddress);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        // Obtener EstacionId desde la navegación (si tiene estación asignada)
        var estacionId = user.Estacion?.Id;

        return AuthResponse.Ok(
            accessToken,
            refreshToken,
            expiresAt,
            new UserInfo
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                NombreCompleto = user.NombreCompleto,
                Rol = primaryRole,
                PuntosAcumulados = user.PuntosAcumulados,
                EstacionId = estacionId
            });
    }

    private string GenerateAccessToken(ApplicationUser user, string role)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(AppClaimTypes.UserId, user.Id),
            new(AppClaimTypes.Email, user.Email ?? string.Empty),
            new(AppClaimTypes.FullName, user.NombreCompleto),
            new(AppClaimTypes.Role, role),
            new(ClaimTypes.Role, role), // Para [Authorize(Roles = "...")]
            new(AppClaimTypes.IsActive, user.Activo.ToString()),
            new(AppClaimTypes.Puntos, user.PuntosAcumulados.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id)
        };

        // Agregar EstacionId si el usuario es barbero y tiene estación asignada
        var estacionId = user.Estacion?.Id;
        if (estacionId.HasValue)
        {
            claims.Add(new Claim(AppClaimTypes.EstacionId, estacionId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateAndStoreRefreshTokenAsync(string userId, string? ipAddress)
    {
        var token = GenerateRefreshTokenString();

        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return token;
    }

    private static string GenerateRefreshTokenString()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateLifetime = false, // Permitir tokens expirados
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    #endregion
}


using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SalonAppointmentSystem.Web.Constants;

namespace SalonAppointmentSystem.Web.Services.Auth;

/// <summary>
/// Implementación del servicio de manejo de tokens JWT
/// Parsea y extrae información del token sin validar la firma
/// </summary>
public class TokenService : ITokenService
{
    private readonly ILogger<TokenService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    /// <summary>
    /// Margen de seguridad antes de la expiración del token (en minutos)
    /// Si el token expira en menos de este tiempo, se considera expirado
    /// </summary>
    private const int ExpirationMarginMinutes = 5;

    public TokenService(ILogger<TokenService> logger)
    {
        _logger = logger;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public IEnumerable<Claim> GetClaimsFromToken(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return Enumerable.Empty<Claim>();
            }

            var jwtToken = _tokenHandler.ReadJwtToken(token);
            return jwtToken.Claims;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al extraer claims del token");
            return Enumerable.Empty<Claim>();
        }
    }

    public string? GetClaimValue(string token, string claimType)
    {
        try
        {
            var claims = GetClaimsFromToken(token);
            return claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener claim {ClaimType} del token", claimType);
            return null;
        }
    }

    public bool IsTokenExpired(string token)
    {
        try
        {
            var expiration = GetTokenExpiration(token);
            if (expiration == null)
            {
                _logger.LogWarning("No se pudo obtener la fecha de expiración del token");
                return true; // Si no podemos determinar la expiración, asumimos que está expirado
            }

            // Considerar el margen de seguridad
            var expirationWithMargin = expiration.Value.AddMinutes(-ExpirationMarginMinutes);
            var isExpired = DateTime.UtcNow >= expirationWithMargin;

            if (isExpired)
            {
                _logger.LogInformation("Token expirado o próximo a expirar. Expira en: {Expiration}", expiration);
            }

            return isExpired;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al verificar expiración del token");
            return true; // En caso de error, asumimos que está expirado por seguridad
        }
    }

    public DateTime? GetTokenExpiration(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var jwtToken = _tokenHandler.ReadJwtToken(token);
            
            // La fecha de expiración está en el claim 'exp' (Unix timestamp)
            return jwtToken.ValidTo;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener fecha de expiración del token");
            return null;
        }
    }

    public string? GetUserRole(string token)
    {
        return GetClaimValue(token, AppClaimTypes.Role);
    }

    public string? GetUserId(string token)
    {
        return GetClaimValue(token, AppClaimTypes.UserId);
    }

    public string? GetUserEmail(string token)
    {
        return GetClaimValue(token, AppClaimTypes.Email);
    }

    public string? GetUserFullName(string token)
    {
        return GetClaimValue(token, AppClaimTypes.FullName);
    }

    public bool IsValidTokenFormat(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            // Verificar que tenga el formato JWT (3 partes separadas por puntos)
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                return false;
            }

            // Intentar leer el token
            _tokenHandler.ReadJwtToken(token);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Token con formato inválido");
            return false;
        }
    }
}


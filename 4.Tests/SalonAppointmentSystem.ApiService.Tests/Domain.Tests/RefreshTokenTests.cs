using FluentAssertions;
using SalonAppointmentSystem.ApiService.Domain.Entities;

namespace SalonAppointmentSystem.ApiService.Tests.Domain.Tests;

/// <summary>
/// Tests unitarios para la entidad RefreshToken
/// </summary>
public class RefreshTokenTests
{
    [Fact]
    public void IsExpired_WhenExpiresAtIsInPast_ShouldReturnTrue()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "test-token",
            UserId = "user-123",
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // Expiró hace 1 hora
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        // Act & Assert
        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenExpiresAtIsInFuture_ShouldReturnFalse()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "test-token",
            UserId = "user-123",
            ExpiresAt = DateTime.UtcNow.AddHours(1), // Expira en 1 hora
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenNotRevokedAndNotExpired_ShouldReturnTrue()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "test-token",
            UserId = "user-123",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };

        // Act & Assert
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenRevoked_ShouldReturnFalse()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "test-token",
            UserId = "user-123",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = DateTime.UtcNow // Token revocado
        };

        // Act & Assert
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "test-token",
            UserId = "user-123",
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // Expirado
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            RevokedAt = null
        };

        // Act & Assert
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAtAndReason()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "test-token",
            UserId = "user-123",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        var reason = "User logout";
        var ip = "192.168.1.1";

        // Act
        token.Revoke(reason, ip);

        // Assert
        token.RevokedAt.Should().NotBeNull();
        token.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        token.ReasonRevoked.Should().Be(reason);
        token.RevokedByIp.Should().Be(ip);
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Revoke_WithReplacedByToken_ShouldSetReplacedByToken()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "old-token",
            UserId = "user-123",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        var newToken = "new-token-value";

        // Act
        token.Revoke("Replaced by new token", null, newToken);

        // Assert
        token.ReplacedByToken.Should().Be(newToken);
    }

    [Fact]
    public void Revoke_WithoutParameters_ShouldStillRevokeToken()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "test-token",
            UserId = "user-123",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        token.Revoke();

        // Assert
        token.RevokedAt.Should().NotBeNull();
        token.IsActive.Should().BeFalse();
        token.ReasonRevoked.Should().BeNull();
        token.RevokedByIp.Should().BeNull();
    }
}


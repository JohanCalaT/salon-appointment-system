using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SalonAppointmentSystem.ApiService.Application.Common.Settings;
using SalonAppointmentSystem.ApiService.Domain.Entities;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence;
using SalonAppointmentSystem.ApiService.Infrastructure.Services;
using SalonAppointmentSystem.Shared.DTOs.Auth;

namespace SalonAppointmentSystem.ApiService.Tests.Infrastructure.Tests;

/// <summary>
/// Tests unitarios para AuthService
/// </summary>
public class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Configurar base de datos en memoria
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Configurar UserManager mock
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        // Configurar JWT settings
        _jwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "TuClaveSecretaSuperSeguraDeAlMenos32CaracteresParaJWT2024!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        });

        // Configurar logger mock
        _loggerMock = new Mock<ILogger<AuthService>>();

        // Crear AuthService
        _authService = new AuthService(
            _userManagerMock.Object,
            _context,
            _jwtSettings,
            _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Login Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccessResponse()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new LoginRequest { Email = user.Email!, Password = "Test123!" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { ApplicationRoles.Cliente });

        // Act
        var result = await _authService.LoginAsync(request, "127.0.0.1");

        // Assert
        result.Success.Should().BeTrue();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldReturnFailResponse()
    {
        // Arrange
        var request = new LoginRequest { Email = "notexist@test.com", Password = "Test123!" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Credenciales inválidas");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldReturnFailResponse()
    {
        // Arrange
        var user = CreateTestUser();
        user.Activo = false;
        var request = new LoginRequest { Email = user.Email!, Password = "Test123!" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("inactivo");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldReturnFailResponse()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new LoginRequest { Email = user.Email!, Password = "WrongPassword!" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Credenciales inválidas");
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task RegisterAsync_WithNewEmail_ShouldReturnSuccessResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@test.com",
            Password = "Test123!",
            NombreCompleto = "Nuevo Usuario",
            PhoneNumber = "1234567890"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((u, p) => u.Id = Guid.NewGuid().ToString());
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), ApplicationRoles.Cliente))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { ApplicationRoles.Cliente });

        // Act
        var result = await _authService.RegisterAsync(request, "127.0.0.1");

        // Assert
        result.Success.Should().BeTrue();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnFailResponse()
    {
        // Arrange
        var existingUser = CreateTestUser();
        var request = new RegisterRequest
        {
            Email = existingUser.Email!,
            Password = "Test123!",
            NombreCompleto = "Test User"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("ya está registrado");
    }

    #endregion

    #region RevokeToken Tests

    [Fact]
    public async Task RevokeTokenAsync_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Token = "valid-refresh-token",
            UserId = "user-123",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.RevokeTokenAsync(refreshToken.Token, "127.0.0.1");

        // Assert
        result.Should().BeTrue();
        var revokedToken = await _context.RefreshTokens.FirstAsync(t => t.Token == refreshToken.Token);
        revokedToken.RevokedAt.Should().NotBeNull();
        revokedToken.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeTokenAsync_WithInvalidToken_ShouldReturnFalse()
    {
        // Act
        var result = await _authService.RevokeTokenAsync("non-existent-token");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeTokenAsync_WithAlreadyRevokedToken_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Token = "already-revoked-token",
            UserId = "user-123",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.RevokeTokenAsync(refreshToken.Token);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RevokeAllUserTokens Tests

    [Fact]
    public async Task RevokeAllUserTokensAsync_ShouldRevokeAllActiveTokens()
    {
        // Arrange
        var userId = "user-456";
        var tokens = new[]
        {
            new RefreshToken { Token = "token-1", UserId = userId, ExpiresAt = DateTime.UtcNow.AddDays(7) },
            new RefreshToken { Token = "token-2", UserId = userId, ExpiresAt = DateTime.UtcNow.AddDays(7) },
            new RefreshToken { Token = "token-3", UserId = userId, ExpiresAt = DateTime.UtcNow.AddDays(7) }
        };
        await _context.RefreshTokens.AddRangeAsync(tokens);
        await _context.SaveChangesAsync();

        // Act
        var count = await _authService.RevokeAllUserTokensAsync(userId, "127.0.0.1");

        // Assert
        count.Should().Be(3);
        var allTokens = await _context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
        allTokens.Should().AllSatisfy(t => t.IsActive.Should().BeFalse());
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithNoTokens_ShouldReturnZero()
    {
        // Act
        var count = await _authService.RevokeAllUserTokensAsync("user-with-no-tokens");

        // Assert
        count.Should().Be(0);
    }

    #endregion

    #region Helper Methods

    private static ApplicationUser CreateTestUser()
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@test.com",
            Email = "test@test.com",
            NombreCompleto = "Test User",
            Activo = true,
            EmailConfirmed = true,
            FechaRegistro = DateTime.UtcNow
        };
    }

    #endregion
}


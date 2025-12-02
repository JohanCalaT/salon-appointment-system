using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.ApiService.Presentation.Authorization;
using System.Security.Claims;

namespace SalonAppointmentSystem.ApiService.Tests.Presentation.Tests.Authorization;

/// <summary>
/// Tests para OperationAuthorizationHandler
/// </summary>
public class OperationAuthorizationHandlerTests
{
    private readonly OperationAuthorizationHandler _handler;
    private readonly Mock<ILogger<OperationAuthorizationHandler>> _loggerMock;

    public OperationAuthorizationHandlerTests()
    {
        _loggerMock = new Mock<ILogger<OperationAuthorizationHandler>>();
        _handler = new OperationAuthorizationHandler(_loggerMock.Object);
    }

    #region Admin Tests

    [Theory]
    [InlineData(UserOperations.Read)]
    [InlineData(UserOperations.Create)]
    [InlineData(UserOperations.Update)]
    [InlineData(UserOperations.Delete)]
    [InlineData(UserOperations.Manage)]
    public async Task Admin_ShouldHaveAccess_ToAllUserOperations(string operation)
    {
        // Arrange
        var user = CreateClaimsPrincipal(ApplicationRoles.Admin);
        var context = CreateAuthorizationContext(user, operation);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(ReservaOperations.Read)]
    [InlineData(ReservaOperations.Create)]
    [InlineData(ReservaOperations.Update)]
    [InlineData(ReservaOperations.Delete)]
    [InlineData(ReservaOperations.Manage)]
    public async Task Admin_ShouldHaveAccess_ToAllReservaOperations(string operation)
    {
        // Arrange
        var user = CreateClaimsPrincipal(ApplicationRoles.Admin);
        var context = CreateAuthorizationContext(user, operation);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    #endregion

    #region Barbero Tests

    [Fact]
    public async Task Barbero_ShouldHaveAccess_ToUserRead()
    {
        // Arrange
        var user = CreateClaimsPrincipal(ApplicationRoles.Barbero);
        var context = CreateAuthorizationContext(user, UserOperations.Read);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(UserOperations.Create)]
    [InlineData(UserOperations.Update)]
    [InlineData(UserOperations.Delete)]
    [InlineData(UserOperations.Manage)]
    public async Task Barbero_ShouldNotHaveAccess_ToUserWriteOperations(string operation)
    {
        // Arrange
        var user = CreateClaimsPrincipal(ApplicationRoles.Barbero);
        var context = CreateAuthorizationContext(user, operation);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Theory]
    [InlineData(ReservaOperations.Read)]
    [InlineData(ReservaOperations.Update)]
    public async Task Barbero_ShouldHaveAccess_ToReservaReadAndUpdate(string operation)
    {
        // Arrange
        var user = CreateClaimsPrincipal(ApplicationRoles.Barbero);
        var context = CreateAuthorizationContext(user, operation);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    #endregion

    #region Cliente Tests

    [Theory]
    [InlineData(UserOperations.Read)]
    [InlineData(UserOperations.Create)]
    [InlineData(UserOperations.Update)]
    [InlineData(UserOperations.Delete)]
    public async Task Cliente_ShouldNotHaveAccess_ToUserOperations(string operation)
    {
        // Arrange
        var user = CreateClaimsPrincipal(ApplicationRoles.Cliente);
        var context = CreateAuthorizationContext(user, operation);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Theory]
    [InlineData(ReservaOperations.Read)]
    [InlineData(ReservaOperations.Create)]
    public async Task Cliente_ShouldHaveAccess_ToReservaReadAndCreate(string operation)
    {
        // Arrange
        var user = CreateClaimsPrincipal(ApplicationRoles.Cliente);
        var context = CreateAuthorizationContext(user, operation);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Cliente_ShouldHaveAccess_ToServicioRead()
    {
        // Arrange
        var user = CreateClaimsPrincipal(ApplicationRoles.Cliente);
        var context = CreateAuthorizationContext(user, ServicioOperations.Read);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    #endregion

    #region Unauthenticated Tests

    [Fact]
    public async Task Unauthenticated_ShouldNotHaveAccess_ToAnyOperation()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // No autenticado
        var context = CreateAuthorizationContext(user, UserOperations.Read);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private static ClaimsPrincipal CreateClaimsPrincipal(string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private static AuthorizationHandlerContext CreateAuthorizationContext(ClaimsPrincipal user, string operation)
    {
        var requirement = new OperationRequirement(operation);
        return new AuthorizationHandlerContext(new[] { requirement }, user, null);
    }

    #endregion
}


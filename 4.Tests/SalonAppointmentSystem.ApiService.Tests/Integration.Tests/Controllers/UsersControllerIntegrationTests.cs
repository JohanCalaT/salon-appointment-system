using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.ApiService.Tests.Integration.Tests.Fixtures;
using SalonAppointmentSystem.Shared.DTOs.Users;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Tests.Integration.Tests.Controllers;

/// <summary>
/// Tests de integración para UsersController
/// Verifica todos los endpoints con diferentes roles y escenarios
/// </summary>
public class UsersControllerIntegrationTests : IntegrationTestBase
{
    public UsersControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region GET /api/users (Lista paginada)

    [Fact]
    public async Task GetPaged_AsAdmin_ShouldReturnUsers()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<PagedResult<UserDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPaged_AsBarbero_ShouldReturnUsers()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();

        // Act
        var response = await Client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<PagedResult<UserDto>>(response);
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetPaged_AsCliente_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsClienteAsync();

        // Act
        var response = await Client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPaged_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        RemoveAuthentication();

        // Act
        var response = await Client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPaged_WithFilters_ShouldFilterResults()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/users?rol=Admin&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<PagedResult<UserDto>>(response);
        result!.Data!.Items.Should().OnlyContain(u => u.Rol == ApplicationRoles.Admin);
    }

    [Fact]
    public async Task GetPaged_WithSearch_ShouldFilterBySearchTerm()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/users?search=admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<PagedResult<UserDto>>(response);
        result!.Success.Should().BeTrue();
    }

    #endregion

    #region GET /api/users/all (Lista completa)

    [Fact]
    public async Task GetAll_AsAdmin_ShouldReturnAllUsers()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/users/all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<IReadOnlyList<UserDto>>(response);
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAll_AsBarbero_ShouldReturnAllUsers()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();

        // Act
        var response = await Client.GetAsync("/api/users/all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_AsCliente_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsClienteAsync();

        // Act
        var response = await Client.GetAsync("/api/users/all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region GET /api/users/{id} (Obtener por ID)

    [Fact]
    public async Task GetById_AsAdmin_WithValidId_ShouldReturnUser()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var adminId = await GetAdminUserIdAsync();

        // Act
        var response = await Client.GetAsync($"/api/users/{adminId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<UserDto>(response);
        result!.Success.Should().BeTrue();
        result.Data!.Email.Should().Be(TestUsers.AdminEmail);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/users/invalid-id-12345");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_AsBarbero_ShouldReturnUser()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();
        var clienteId = await GetClienteUserIdAsync();

        // Act
        var response = await Client.GetAsync($"/api/users/{clienteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region POST /api/users (Crear usuario)

    [Fact]
    public async Task Create_AsAdmin_WithValidData_ShouldCreateUser()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var request = new CreateUserRequest
        {
            Email = $"nuevo.usuario.{Guid.NewGuid():N}@test.com",
            Password = "NuevaPass123",
            ConfirmPassword = "NuevaPass123",
            NombreCompleto = "Nuevo Usuario Test",
            Rol = ApplicationRoles.Cliente,
            Activo = true
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await DeserializeResponseAsync<UserDto>(response);
        result!.Success.Should().BeTrue();
        result.Data!.Email.Should().Be(request.Email);
        result.Data.NombreCompleto.Should().Be(request.NombreCompleto);
        result.Data.Rol.Should().Be(ApplicationRoles.Cliente);
    }

    [Fact]
    public async Task Create_AsAdmin_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var request = new CreateUserRequest
        {
            Email = TestUsers.AdminEmail, // Email ya existente
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            NombreCompleto = "Usuario Duplicado",
            Rol = ApplicationRoles.Cliente
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_AsAdmin_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var request = new CreateUserRequest
        {
            Email = "email-invalido",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            NombreCompleto = "Test",
            Rol = ApplicationRoles.Cliente
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_AsBarbero_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();
        var request = new CreateUserRequest
        {
            Email = "intento@test.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            NombreCompleto = "Intento",
            Rol = ApplicationRoles.Cliente
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_AsCliente_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsClienteAsync();
        var request = new CreateUserRequest
        {
            Email = "intento@test.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            NombreCompleto = "Intento",
            Rol = ApplicationRoles.Cliente
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_WithPasswordMismatch_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var request = new CreateUserRequest
        {
            Email = "nuevo@test.com",
            Password = "Test123!",
            ConfirmPassword = "OtraPassword123!",
            NombreCompleto = "Test",
            Rol = ApplicationRoles.Cliente
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithInvalidRole_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var request = new CreateUserRequest
        {
            Email = "nuevo@test.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            NombreCompleto = "Test",
            Rol = "RolInvalido"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/users/{id} (Actualizar usuario completo)

    [Fact]
    public async Task Update_AsAdmin_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Primero crear un usuario para actualizar
        var createRequest = new CreateUserRequest
        {
            Email = $"update.test.{Guid.NewGuid():N}@test.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            NombreCompleto = "Usuario Original",
            Rol = ApplicationRoles.Cliente,
            Activo = true
        };
        var createResponse = await Client.PostAsJsonAsync("/api/users", createRequest);
        var createdUser = (await DeserializeResponseAsync<UserDto>(createResponse))!.Data!;

        // Actualizar
        var updateRequest = new UpdateUserRequest
        {
            Email = createdUser.Email,
            NombreCompleto = "Usuario Actualizado",
            Rol = ApplicationRoles.Cliente,
            Activo = true,
            PuntosAcumulados = 50
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/users/{createdUser.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<UserDto>(response);
        result!.Data!.NombreCompleto.Should().Be("Usuario Actualizado");
        result.Data.PuntosAcumulados.Should().Be(50);
    }

    [Fact]
    public async Task Update_AsAdmin_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var updateRequest = new UpdateUserRequest
        {
            Email = "test@test.com",
            NombreCompleto = "Test",
            Rol = ApplicationRoles.Cliente,
            Activo = true
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/users/non-existent-id", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_AsBarbero_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();
        var clienteId = await GetClienteUserIdAsync();
        var updateRequest = new UpdateUserRequest
        {
            Email = "nuevo@test.com",
            NombreCompleto = "Intento",
            Rol = ApplicationRoles.Cliente,
            Activo = true
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/users/{clienteId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region PATCH /api/users/{id} (Actualización parcial)

    [Fact]
    public async Task Patch_AsAdmin_ShouldUpdateOnlyProvidedFields()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var clienteId = await GetClienteUserIdAsync();
        var patchRequest = new PatchUserRequest
        {
            NombreCompleto = "Nombre Parcialmente Actualizado"
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/users/{clienteId}", patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<UserDto>(response);
        result!.Data!.NombreCompleto.Should().Be("Nombre Parcialmente Actualizado");
    }

    [Fact]
    public async Task Patch_AsAdmin_WithPuntosUpdate_ShouldUpdatePuntos()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var clienteId = await GetClienteUserIdAsync();
        var patchRequest = new PatchUserRequest
        {
            PuntosAcumulados = 100
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/users/{clienteId}", patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<UserDto>(response);
        result!.Data!.PuntosAcumulados.Should().Be(100);
    }

    [Fact]
    public async Task Patch_AsBarbero_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();
        var clienteId = await GetClienteUserIdAsync();
        var patchRequest = new PatchUserRequest
        {
            NombreCompleto = "Intento de Patch"
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/users/{clienteId}", patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region DELETE /api/users/{id}

    [Fact]
    public async Task Delete_AsAdmin_WithValidId_ShouldSoftDeleteUser()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Crear usuario para eliminar
        var createRequest = new CreateUserRequest
        {
            Email = $"delete.test.{Guid.NewGuid():N}@test.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            NombreCompleto = "Usuario a Eliminar",
            Rol = ApplicationRoles.Cliente,
            Activo = true
        };
        var createResponse = await Client.PostAsJsonAsync("/api/users", createRequest);
        var createdUser = (await DeserializeResponseAsync<UserDto>(createResponse))!.Data!;

        // Act
        var response = await Client.DeleteAsync($"/api/users/{createdUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que el usuario ya no está activo
        var getResponse = await Client.GetAsync($"/api/users/{createdUser.Id}");
        var userAfterDelete = (await DeserializeResponseAsync<UserDto>(getResponse))!.Data!;
        userAfterDelete.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_AsAdmin_SelfDelete_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var adminId = await GetAdminUserIdAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/users/{adminId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_AsBarbero_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();
        var clienteId = await GetClienteUserIdAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/users/{clienteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region POST /api/users/{id}/toggle-active

    [Fact]
    public async Task ToggleActive_AsAdmin_ShouldToggleUserActiveStatus()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Crear usuario para toggle
        var createRequest = new CreateUserRequest
        {
            Email = $"toggle.test.{Guid.NewGuid():N}@test.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            NombreCompleto = "Usuario Toggle",
            Rol = ApplicationRoles.Cliente,
            Activo = true
        };
        var createResponse = await Client.PostAsJsonAsync("/api/users", createRequest);
        var createdUser = (await DeserializeResponseAsync<UserDto>(createResponse))!.Data!;

        // Act - Desactivar
        var response = await Client.PostAsync($"/api/users/{createdUser.Id}/toggle-active", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<bool>(response);
        result!.Data.Should().BeFalse(); // Ahora está inactivo

        // Act - Reactivar
        var response2 = await Client.PostAsync($"/api/users/{createdUser.Id}/toggle-active", null);
        var result2 = await DeserializeResponseAsync<bool>(response2);
        result2!.Data.Should().BeTrue(); // Ahora está activo de nuevo
    }

    [Fact]
    public async Task ToggleActive_AsAdmin_SelfToggle_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var adminId = await GetAdminUserIdAsync();

        // Act
        var response = await Client.PostAsync($"/api/users/{adminId}/toggle-active", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region POST /api/users/{id}/reset-password

    [Fact]
    public async Task ResetPassword_AsAdmin_WithValidData_ShouldResetPassword()
    {
        // Arrange - Crear un usuario específico para este test
        await AuthenticateAsAdminAsync();
        var uniqueId = Guid.NewGuid().ToString("N");
        var createRequest = new CreateUserRequest
        {
            Email = $"reset.test.{uniqueId}@test.com",
            Password = "OldPassword123!",
            ConfirmPassword = "OldPassword123!",
            NombreCompleto = "Usuario Reset Test",
            Rol = ApplicationRoles.Cliente
        };
        var createResponse = await Client.PostAsJsonAsync("/api/users", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createResult = await DeserializeResponseAsync<UserDto>(createResponse);
        var userId = createResult!.Data!.Id;

        var request = new ResetPasswordRequest
        {
            NewPassword = "NuevaPassword123!",
            ConfirmPassword = "NuevaPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/users/{userId}/reset-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<bool>(response);
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPassword_AsBarbero_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();
        // Usar el ID del barbero mismo (no importa el ID, solo verificamos que retorne Forbidden)
        var barberoId = await GetBarberoUserIdAsync();
        var request = new ResetPasswordRequest
        {
            NewPassword = "NuevaPassword123!",
            ConfirmPassword = "NuevaPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/users/{barberoId}/reset-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}


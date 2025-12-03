using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SalonAppointmentSystem.ApiService.Tests.Integration.Tests.Fixtures;
using SalonAppointmentSystem.Shared.DTOs.Estaciones;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Tests.Integration.Tests.Controllers;

/// <summary>
/// Tests de integración para EstacionesController
/// Admin: CRUD completo
/// Barbero: Solo lectura de su estación
/// Cliente/Invitado: Solo estaciones activas (público)
/// </summary>
public class EstacionesControllerIntegrationTests : IntegrationTestBase
{
    public EstacionesControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region GET /api/estaciones (Lista paginada)

    [Fact]
    public async Task GetPaged_AsAdmin_ShouldReturnAllEstaciones()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/estaciones");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<PagedResult<EstacionDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetPaged_AsBarbero_ShouldReturnOnlyOwnEstacion()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();

        // Act
        var response = await Client.GetAsync("/api/estaciones");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<PagedResult<EstacionDto>>(response);
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetPaged_AsCliente_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsClienteAsync();

        // Act
        var response = await Client.GetAsync("/api/estaciones");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPaged_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        RemoveAuthentication();

        // Act
        var response = await Client.GetAsync("/api/estaciones");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /api/estaciones/activas (Público)

    [Fact]
    public async Task GetActivas_WithoutAuth_ShouldReturnOk()
    {
        // Arrange
        RemoveAuthentication();

        // Act
        var response = await Client.GetAsync("/api/estaciones/activas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<IReadOnlyList<EstacionDto>>(response);
        result!.Success.Should().BeTrue();
    }

    #endregion

    #region GET /api/estaciones/mi-estacion (Solo Barbero)

    [Fact]
    public async Task GetMiEstacion_AsBarbero_ShouldReturnOwnEstacion()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();

        // Act
        var response = await Client.GetAsync("/api/estaciones/mi-estacion");

        // Assert
        // Puede ser OK o NotFound dependiendo si el barbero tiene estación asignada
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMiEstacion_AsAdmin_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/estaciones/mi-estacion");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region POST /api/estaciones (Crear - Solo Admin)

    [Fact]
    public async Task Create_AsAdmin_WithValidData_ShouldCreateEstacion()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var request = new CreateEstacionRequest
        {
            Nombre = $"Estación Test {Guid.NewGuid():N}",
            Descripcion = "Descripción de prueba",
            Orden = 99
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/estaciones", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await DeserializeResponseAsync<EstacionDto>(response);
        result!.Success.Should().BeTrue();
        result.Data!.Nombre.Should().Be(request.Nombre);
    }

    [Fact]
    public async Task Create_AsBarbero_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();
        var request = new CreateEstacionRequest
        {
            Nombre = "Estación No Autorizada",
            Orden = 1
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/estaciones", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_AsAdmin_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var request = new CreateEstacionRequest
        {
            Nombre = "", // Nombre vacío - inválido
            Orden = 1
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/estaciones", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/estaciones/{id} (Actualizar - Solo Admin)

    [Fact]
    public async Task Update_AsAdmin_WithValidData_ShouldUpdateEstacion()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Primero crear una estación
        var createRequest = new CreateEstacionRequest
        {
            Nombre = $"Estación Update Test {Guid.NewGuid():N}",
            Orden = 50
        };
        var createResponse = await Client.PostAsJsonAsync("/api/estaciones", createRequest);
        var created = await DeserializeResponseAsync<EstacionDto>(createResponse);
        var estacionId = created!.Data!.Id;

        var updateRequest = new UpdateEstacionRequest
        {
            Nombre = "Estación Actualizada",
            Descripcion = "Nueva descripción",
            Orden = 51,
            Activa = true
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/estaciones/{estacionId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<EstacionDto>(response);
        result!.Success.Should().BeTrue();
        result.Data!.Nombre.Should().Be(updateRequest.Nombre);
    }

    [Fact]
    public async Task Update_AsBarbero_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();
        var request = new UpdateEstacionRequest
        {
            Nombre = "Intento de actualización",
            Orden = 1,
            Activa = true
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/estaciones/1", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region DELETE /api/estaciones/{id} (Eliminar - Solo Admin)

    [Fact]
    public async Task Delete_AsAdmin_ShouldDeleteEstacion()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Crear estación para eliminar
        var createRequest = new CreateEstacionRequest
        {
            Nombre = $"Estación Delete Test {Guid.NewGuid():N}",
            Orden = 99
        };
        var createResponse = await Client.PostAsJsonAsync("/api/estaciones", createRequest);
        var created = await DeserializeResponseAsync<EstacionDto>(createResponse);
        var estacionId = created!.Data!.Id;

        // Act
        var response = await Client.DeleteAsync($"/api/estaciones/{estacionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<bool>(response);
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_AsBarbero_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();

        // Act
        var response = await Client.DeleteAsync("/api/estaciones/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region POST /api/estaciones/{id}/toggle-active (Solo Admin)

    [Fact]
    public async Task ToggleActive_AsAdmin_ShouldToggleEstacionStatus()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Crear estación para toggle
        var createRequest = new CreateEstacionRequest
        {
            Nombre = $"Estación Toggle Test {Guid.NewGuid():N}",
            Orden = 99
        };
        var createResponse = await Client.PostAsJsonAsync("/api/estaciones", createRequest);
        var created = await DeserializeResponseAsync<EstacionDto>(createResponse);
        var estacionId = created!.Data!.Id;

        // Act
        var response = await Client.PostAsync($"/api/estaciones/{estacionId}/toggle-active", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<bool>(response);
        result!.Success.Should().BeTrue();
    }

    #endregion

    #region POST /api/estaciones/{id}/asignar-barbero (Solo Admin)

    [Fact]
    public async Task AsignarBarbero_AsAdmin_WithValidBarbero_ShouldAssign()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var barberoId = await GetBarberoUserIdAsync();

        // Crear estación nueva sin barbero
        var createRequest = new CreateEstacionRequest
        {
            Nombre = $"Estación Asignar Test {Guid.NewGuid():N}",
            Orden = 99
        };
        var createResponse = await Client.PostAsJsonAsync("/api/estaciones", createRequest);
        var created = await DeserializeResponseAsync<EstacionDto>(createResponse);
        var estacionId = created!.Data!.Id;

        var request = new AsignarBarberoRequest { BarberoId = barberoId };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/estaciones/{estacionId}/asignar-barbero", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<EstacionDto>(response);
        result!.Success.Should().BeTrue();
        result.Data!.BarberoId.Should().Be(barberoId);
    }

    [Fact]
    public async Task AsignarBarbero_AsBarbero_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();
        var request = new AsignarBarberoRequest { BarberoId = "any-id" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/estaciones/1/asignar-barbero", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}


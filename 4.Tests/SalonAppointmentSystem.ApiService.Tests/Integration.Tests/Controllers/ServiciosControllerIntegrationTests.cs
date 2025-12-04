using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SalonAppointmentSystem.ApiService.Tests.Integration.Tests.Fixtures;
using SalonAppointmentSystem.Shared.DTOs.Servicios;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Tests.Integration.Tests.Controllers;

/// <summary>
/// Tests de integración para ServiciosController
/// Admin y Barbero: CRUD completo
/// Cliente/Invitado: Sin acceso
/// </summary>
public class ServiciosControllerIntegrationTests : IntegrationTestBase
{
    public ServiciosControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region GET /api/servicios (Lista paginada)

    [Fact]
    public async Task GetPaged_AsAdmin_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/servicios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<PagedResult<ServicioDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetPaged_AsBarbero_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();

        // Act
        var response = await Client.GetAsync("/api/servicios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<PagedResult<ServicioDto>>(response);
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetPaged_AsCliente_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsClienteAsync();

        // Act
        var response = await Client.GetAsync("/api/servicios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPaged_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        RemoveAuthentication();

        // Act
        var response = await Client.GetAsync("/api/servicios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /api/servicios/activos

    [Fact]
    public async Task GetActivos_AsAdmin_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/servicios/activos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<IReadOnlyList<ServicioDto>>(response);
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetActivos_AsBarbero_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();

        // Act
        var response = await Client.GetAsync("/api/servicios/activos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetActivos_AsCliente_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsClienteAsync();

        // Act
        var response = await Client.GetAsync("/api/servicios/activos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region POST /api/servicios (Crear)

    [Fact]
    public async Task Create_AsAdmin_WithValidData_ShouldCreateServicio()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var request = new CreateServicioRequest
        {
            Nombre = $"Servicio Test {Guid.NewGuid():N}",
            Descripcion = "Descripción de prueba",
            DuracionMinutos = 30,
            Precio = 15.50m,
            PuntosQueOtorga = 10,
            Orden = 1
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/servicios", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await DeserializeResponseAsync<ServicioDto>(response);
        result!.Success.Should().BeTrue();
        result.Data!.Nombre.Should().Be(request.Nombre);
        result.Data.Precio.Should().Be(request.Precio);
    }

    [Fact]
    public async Task Create_AsBarbero_WithValidData_ShouldCreateServicio()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();
        var request = new CreateServicioRequest
        {
            Nombre = $"Servicio Barbero {Guid.NewGuid():N}",
            Descripcion = "Creado por barbero",
            DuracionMinutos = 45,
            Precio = 20.00m,
            PuntosQueOtorga = 15,
            Orden = 2
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/servicios", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await DeserializeResponseAsync<ServicioDto>(response);
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Create_AsCliente_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsClienteAsync();
        var request = new CreateServicioRequest
        {
            Nombre = "Servicio No Autorizado",
            DuracionMinutos = 30,
            Precio = 10.00m
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/servicios", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var request = new CreateServicioRequest
        {
            Nombre = "", // Nombre vacío - inválido
            DuracionMinutos = 0, // Duración inválida
            Precio = -5 // Precio negativo
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/servicios", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/servicios/{id} (Actualizar)

    [Fact]
    public async Task Update_AsAdmin_WithValidData_ShouldUpdateServicio()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Primero crear un servicio
        var createRequest = new CreateServicioRequest
        {
            Nombre = $"Servicio Update Test {Guid.NewGuid():N}",
            DuracionMinutos = 30,
            Precio = 15.00m
        };
        var createResponse = await Client.PostAsJsonAsync("/api/servicios", createRequest);
        var created = await DeserializeResponseAsync<ServicioDto>(createResponse);
        var servicioId = created!.Data!.Id;

        // Actualizar
        var updateRequest = new UpdateServicioRequest
        {
            Nombre = $"Servicio Actualizado {Guid.NewGuid():N}",
            Descripcion = "Nueva descripción",
            DuracionMinutos = 45,
            Precio = 25.00m,
            PuntosQueOtorga = 20,
            Activo = true,
            Orden = 5
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/servicios/{servicioId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<ServicioDto>(response);
        result!.Success.Should().BeTrue();
        result.Data!.Precio.Should().Be(25.00m);
    }

    [Fact]
    public async Task Update_AsBarbero_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Crear servicio como admin
        var createRequest = new CreateServicioRequest
        {
            Nombre = $"Servicio Barbero Update {Guid.NewGuid():N}",
            DuracionMinutos = 30,
            Precio = 15.00m
        };
        var createResponse = await Client.PostAsJsonAsync("/api/servicios", createRequest);
        var created = await DeserializeResponseAsync<ServicioDto>(createResponse);
        var servicioId = created!.Data!.Id;

        // Cambiar a barbero
        await AuthenticateAsBarberoAsync();

        var updateRequest = new UpdateServicioRequest
        {
            Nombre = $"Actualizado por Barbero {Guid.NewGuid():N}",
            DuracionMinutos = 60,
            Precio = 30.00m,
            Activo = true,
            Orden = 1
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/servicios/{servicioId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_NonExistent_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var updateRequest = new UpdateServicioRequest
        {
            Nombre = "No Existe",
            DuracionMinutos = 30,
            Precio = 10.00m,
            Activo = true,
            Orden = 1
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/servicios/99999", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region PATCH /api/servicios/{id} (Actualización parcial)

    [Fact]
    public async Task Patch_AsAdmin_ShouldUpdatePartially()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Crear servicio
        var createRequest = new CreateServicioRequest
        {
            Nombre = $"Servicio Patch Test {Guid.NewGuid():N}",
            Descripcion = "Descripción original",
            DuracionMinutos = 30,
            Precio = 15.00m
        };
        var createResponse = await Client.PostAsJsonAsync("/api/servicios", createRequest);
        var created = await DeserializeResponseAsync<ServicioDto>(createResponse);
        var servicioId = created!.Data!.Id;

        // Patch solo el precio
        var patchRequest = new PatchServicioRequest
        {
            Precio = 50.00m
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/servicios/{servicioId}", patchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<ServicioDto>(response);
        result!.Success.Should().BeTrue();
        result.Data!.Precio.Should().Be(50.00m);
        result.Data.Descripcion.Should().Be("Descripción original"); // No cambió
    }

    #endregion

    #region DELETE /api/servicios/{id} (Soft delete)

    [Fact]
    public async Task Delete_AsAdmin_ShouldSoftDelete()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Crear servicio
        var createRequest = new CreateServicioRequest
        {
            Nombre = $"Servicio Delete Test {Guid.NewGuid():N}",
            DuracionMinutos = 30,
            Precio = 15.00m
        };
        var createResponse = await Client.PostAsJsonAsync("/api/servicios", createRequest);
        var created = await DeserializeResponseAsync<ServicioDto>(createResponse);
        var servicioId = created!.Data!.Id;

        // Act
        var response = await Client.DeleteAsync($"/api/servicios/{servicioId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<bool>(response);
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_AsCliente_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsClienteAsync();

        // Act
        var response = await Client.DeleteAsync("/api/servicios/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region POST /api/servicios/{id}/toggle-activo

    [Fact]
    public async Task ToggleActivo_AsAdmin_ShouldToggle()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Crear servicio
        var createRequest = new CreateServicioRequest
        {
            Nombre = $"Servicio Toggle Test {Guid.NewGuid():N}",
            DuracionMinutos = 30,
            Precio = 15.00m
        };
        var createResponse = await Client.PostAsJsonAsync("/api/servicios", createRequest);
        var created = await DeserializeResponseAsync<ServicioDto>(createResponse);
        var servicioId = created!.Data!.Id;

        // Act
        var response = await Client.PostAsync($"/api/servicios/{servicioId}/toggle-activo", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<bool>(response);
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleActivo_AsBarbero_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Crear servicio como admin
        var createRequest = new CreateServicioRequest
        {
            Nombre = $"Servicio Toggle Barbero {Guid.NewGuid():N}",
            DuracionMinutos = 30,
            Precio = 15.00m
        };
        var createResponse = await Client.PostAsJsonAsync("/api/servicios", createRequest);
        var created = await DeserializeResponseAsync<ServicioDto>(createResponse);
        var servicioId = created!.Data!.Id;

        // Cambiar a barbero
        await AuthenticateAsBarberoAsync();

        // Act
        var response = await Client.PostAsync($"/api/servicios/{servicioId}/toggle-activo", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ToggleActivo_NonExistent_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.PostAsync("/api/servicios/99999/toggle-activo", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}


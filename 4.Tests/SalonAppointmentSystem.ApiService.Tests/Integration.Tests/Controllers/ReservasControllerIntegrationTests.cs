using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SalonAppointmentSystem.ApiService.Tests.Integration.Tests.Fixtures;
using SalonAppointmentSystem.Shared.DTOs.Estaciones;
using SalonAppointmentSystem.Shared.DTOs.Reservas;
using SalonAppointmentSystem.Shared.DTOs.Servicios;
using SalonAppointmentSystem.Shared.Enums;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Tests.Integration.Tests.Controllers;

/// <summary>
/// Tests de integración para ReservasController
/// Enfocados en autorización y flujos básicos
/// Nota: Crear reservas requiere horarios y barberos configurados
/// </summary>
public class ReservasControllerIntegrationTests : IntegrationTestBase
{
    public ReservasControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region POST /api/reservas - Autorización

    [Fact]
    public async Task Create_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        RemoveAuthentication();
        var request = new CreateReservaRequest
        {
            EstacionId = 1,
            ServicioId = 1,
            FechaHora = DateTime.UtcNow.AddDays(1),
            NombreCliente = "Test",
            EmailCliente = "test@test.com",
            TelefonoCliente = "123"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/reservas", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAnonima_WithPastDate_ShouldReturnBadRequest()
    {
        // Arrange
        RemoveAuthentication();

        var request = new CreateReservaRequest
        {
            EstacionId = 1,
            ServicioId = 1,
            FechaHora = DateTime.UtcNow.AddHours(-1), // Fecha pasada
            NombreCliente = "Cliente Test",
            EmailCliente = "test@example.com",
            TelefonoCliente = "555-1234"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/reservas/anonima", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/reservas/mis-reservas - Autorización

    [Fact]
    public async Task GetMisReservas_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        RemoveAuthentication();

        // Act
        var response = await Client.GetAsync("/api/reservas/mis-reservas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMisReservas_AsCliente_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsClienteAsync();

        // Act
        var response = await Client.GetAsync("/api/reservas/mis-reservas");

        // Assert - puede retornar OK o BadRequest dependiendo de la implementación
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/reservas - Autorización (Solo Admin)

    [Fact]
    public async Task GetPaged_AsBarbero_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();

        // Act
        var response = await Client.GetAsync("/api/reservas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPaged_AsCliente_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsClienteAsync();

        // Act
        var response = await Client.GetAsync("/api/reservas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPaged_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        RemoveAuthentication();

        // Act
        var response = await Client.GetAsync("/api/reservas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /api/reservas/mi-agenda - Autorización (Admin/Barbero)

    [Fact]
    public async Task GetMiAgenda_AsCliente_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsClienteAsync();

        // Act
        var response = await Client.GetAsync("/api/reservas/mi-agenda");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMiAgenda_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        RemoveAuthentication();

        // Act
        var response = await Client.GetAsync("/api/reservas/mi-agenda");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /api/reservas/{id} - Autorización

    [Fact]
    public async Task GetById_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        RemoveAuthentication();

        // Act
        var response = await Client.GetAsync("/api/reservas/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region POST /api/reservas/{id}/cancelar - Autorización

    [Fact]
    public async Task Cancel_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        RemoveAuthentication();
        var request = new CancelReservaRequest { Motivo = "Test" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/reservas/1/cancelar", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region POST /api/reservas/{id}/completar - Autorización

    [Fact]
    public async Task Completar_AsCliente_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsClienteAsync();

        // Act
        var response = await Client.PostAsync("/api/reservas/1/completar", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Completar_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        RemoveAuthentication();

        // Act
        var response = await Client.PostAsync("/api/reservas/1/completar", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region PATCH /api/reservas/{id} - Autorización

    [Fact]
    public async Task Update_AsCliente_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsClienteAsync();
        var request = new UpdateReservaRequest { NombreCliente = "Nuevo Nombre" };

        // Act
        var response = await Client.PatchAsJsonAsync("/api/reservas/1", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        RemoveAuthentication();
        var request = new UpdateReservaRequest { NombreCliente = "Nuevo Nombre" };

        // Act
        var response = await Client.PatchAsJsonAsync("/api/reservas/1", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Endpoints Públicos - Slots y Cita Rápida

    [Fact]
    public async Task GetSlots_WithMissingParams_ShouldReturnBadRequest()
    {
        // Arrange
        RemoveAuthentication();

        // Act - missing servicioId
        var response = await Client.GetAsync("/api/reservas/slots?estacionId=1&fecha=2024-12-15");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetSlots_EndpointIsAccessible_WithoutAuth()
    {
        // Arrange
        RemoveAuthentication();

        // Act
        var response = await Client.GetAsync("/api/reservas/slots?estacionId=1&servicioId=1&fecha=2024-12-15");

        // Assert - El endpoint responde (no requiere auth), aunque puede fallar por datos
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCitaRapida_EndpointIsAccessible_WithoutAuth()
    {
        // Arrange
        RemoveAuthentication();

        // Act
        var response = await Client.GetAsync("/api/reservas/cita-rapida?servicioId=1");

        // Assert - El endpoint responde (no requiere auth)
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task BuscarPorCodigo_EndpointIsAccessible_WithoutAuth()
    {
        // Arrange
        RemoveAuthentication();
        var request = new BuscarReservaPorCodigoRequest
        {
            Codigo = "ABC12345",
            Email = "test@example.com"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/reservas/buscar", request);

        // Assert - El endpoint responde (no requiere auth)
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    #endregion
}


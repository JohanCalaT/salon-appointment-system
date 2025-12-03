using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SalonAppointmentSystem.ApiService.Tests.Integration.Tests.Fixtures;
using SalonAppointmentSystem.Shared.DTOs.Estaciones;
using SalonAppointmentSystem.Shared.DTOs.Horarios;
using SalonAppointmentSystem.Shared.Enums;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Tests.Integration.Tests.Controllers;

/// <summary>
/// Tests de integración para HorariosController
/// Admin: Gestiona horarios globales y de todas las estaciones
/// Barbero: Solo gestiona horarios de su estación
/// </summary>
public class HorariosControllerIntegrationTests : IntegrationTestBase
{
    public HorariosControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region GET /api/horarios/global (Solo Admin)

    [Fact]
    public async Task GetHorarioGlobal_AsAdmin_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/horarios/global");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<HorarioSemanalDto>(response);
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetHorarioGlobal_AsBarbero_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();

        // Act
        var response = await Client.GetAsync("/api/horarios/global");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetHorarioGlobal_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        RemoveAuthentication();

        // Act
        var response = await Client.GetAsync("/api/horarios/global");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region PUT /api/horarios/global (Solo Admin)

    [Fact]
    public async Task ConfigurarHorarioGlobal_AsAdmin_ShouldUpdateHorario()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var request = CreateHorarioSemanalRequest();

        // Act
        var response = await Client.PutAsJsonAsync("/api/horarios/global", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<HorarioSemanalDto>(response);
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ConfigurarHorarioGlobal_AsBarbero_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();
        var request = CreateHorarioSemanalRequest();

        // Act
        var response = await Client.PutAsJsonAsync("/api/horarios/global", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region GET /api/horarios/estacion/{id}/efectivo (Público)

    [Fact]
    public async Task GetHorarioEfectivo_WithoutAuth_ShouldReturnOk()
    {
        // Arrange
        RemoveAuthentication();
        var fecha = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");

        // Act - Usamos estación 1 que debería existir por seed
        var response = await Client.GetAsync($"/api/horarios/estacion/1/efectivo?fecha={fecha}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GET /api/horarios/estacion/{id}/disponible (Público)

    [Fact]
    public async Task EstaDisponible_WithoutAuth_ShouldReturnOk()
    {
        // Arrange
        RemoveAuthentication();
        var fechaHora = DateTime.Today.AddDays(1).AddHours(10).ToString("yyyy-MM-ddTHH:mm:ss");

        // Act
        var response = await Client.GetAsync($"/api/horarios/estacion/1/disponible?fechaHora={fechaHora}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Horarios por Estación

    [Fact]
    public async Task GetHorarioEstacion_AsAdmin_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/horarios/estacion/1");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetHorarioEstacion_AsBarberoDeOtraEstacion_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsBarberoAsync();

        // Act - Intentar acceder a estación 999 que no es del barbero
        var response = await Client.GetAsync("/api/horarios/estacion/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region POST /api/horarios/estacion/{id}/bloquear-dia

    [Fact]
    public async Task BloquearDia_AsAdmin_ShouldBlockDay()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Primero crear una estación para bloquear
        var createRequest = new CreateEstacionRequest
        {
            Nombre = $"Estación Bloqueo Test {Guid.NewGuid():N}",
            Orden = 99
        };
        var createResponse = await Client.PostAsJsonAsync("/api/estaciones", createRequest);
        var created = await DeserializeResponseAsync<EstacionDto>(createResponse);
        var estacionId = created!.Data!.Id;

        var request = new BloquearDiaRequest
        {
            Fecha = DateTime.Today.AddDays(7),
            Motivo = "Día de prueba bloqueado"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/horarios/estacion/{estacionId}/bloquear-dia", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await DeserializeResponseAsync<HorarioDto>(response);
        result!.Success.Should().BeTrue();
        result.Data!.Tipo.Should().Be(TipoHorario.Bloqueado);
    }

    [Fact]
    public async Task BloquearDia_AsCliente_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsClienteAsync();
        var request = new BloquearDiaRequest
        {
            Fecha = DateTime.Today.AddDays(7),
            Motivo = "Intento no autorizado"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/horarios/estacion/1/bloquear-dia", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region POST /api/horarios/estacion/{id}/especiales

    [Fact]
    public async Task CrearHorarioEspecial_AsAdmin_ShouldCreateHorario()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Primero crear una estación
        var createRequest = new CreateEstacionRequest
        {
            Nombre = $"Estación Especial Test {Guid.NewGuid():N}",
            Orden = 99
        };
        var createResponse = await Client.PostAsJsonAsync("/api/estaciones", createRequest);
        var created = await DeserializeResponseAsync<EstacionDto>(createResponse);
        var estacionId = created!.Data!.Id;

        var request = new HorarioEspecialRequest
        {
            Tipo = TipoHorario.Especial,
            FechaDesde = DateTime.Today.AddDays(14),
            FechaHasta = DateTime.Today.AddDays(14),
            HoraInicio = new TimeSpan(8, 0, 0),
            HoraFin = new TimeSpan(20, 0, 0),
            Descripcion = "Horario extendido por evento"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/horarios/estacion/{estacionId}/especiales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await DeserializeResponseAsync<HorarioDto>(response);
        result!.Success.Should().BeTrue();
        result.Data!.Tipo.Should().Be(TipoHorario.Especial);
    }

    #endregion

    #region POST /api/horarios/estacion/{id}/cambiar-tipo

    [Fact]
    public async Task CambiarTipoHorario_AsAdmin_ShouldChangeType()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Primero crear una estación
        var createRequest = new CreateEstacionRequest
        {
            Nombre = $"Estación CambioTipo Test {Guid.NewGuid():N}",
            Orden = 99
        };
        var createResponse = await Client.PostAsJsonAsync("/api/estaciones", createRequest);
        var created = await DeserializeResponseAsync<EstacionDto>(createResponse);
        var estacionId = created!.Data!.Id;

        var request = new CambiarTipoHorarioRequest
        {
            UsaHorarioGenerico = false
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/horarios/estacion/{estacionId}/cambiar-tipo", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Helpers

    private static ConfigurarHorarioSemanalRequest CreateHorarioSemanalRequest()
    {
        var dias = new List<HorarioDiaRequest>();

        for (int i = 0; i < 7; i++)
        {
            var diaSemana = (DayOfWeek)i;
            var trabaja = diaSemana != DayOfWeek.Sunday; // Domingo cerrado

            dias.Add(new HorarioDiaRequest
            {
                DiaSemana = diaSemana,
                Trabaja = trabaja,
                HoraInicio = trabaja ? new TimeSpan(9, 0, 0) : null,
                HoraFin = trabaja ? new TimeSpan(19, 0, 0) : null
            });
        }

        return new ConfigurarHorarioSemanalRequest { Dias = dias };
    }

    #endregion
}


using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using SalonAppointmentSystem.ApiService.Infrastructure.Persistence;
using SalonAppointmentSystem.ApiService.Tests.Integration.Tests.Fixtures;
using SalonAppointmentSystem.Shared.DTOs.Auth;
using SalonAppointmentSystem.Shared.Models;

namespace SalonAppointmentSystem.ApiService.Tests.Integration.Tests;

/// <summary>
/// Clase base para tests de integración
/// Proporciona helpers para autenticación y utilidades comunes
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly JsonSerializerOptions JsonOptions;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Autentica el cliente HTTP con un usuario específico
    /// </summary>
    protected async Task AuthenticateAsync(string email, string password)
    {
        var token = await GetJwtTokenAsync(email, password);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Autentica como Admin
    /// </summary>
    protected Task AuthenticateAsAdminAsync()
        => AuthenticateAsync(TestUsers.AdminEmail, TestUsers.DefaultPassword);

    /// <summary>
    /// Autentica como Barbero
    /// </summary>
    protected Task AuthenticateAsBarberoAsync()
        => AuthenticateAsync(TestUsers.BarberoEmail, TestUsers.DefaultPassword);

    /// <summary>
    /// Autentica como Cliente
    /// </summary>
    protected Task AuthenticateAsClienteAsync()
        => AuthenticateAsync(TestUsers.ClienteEmail, TestUsers.DefaultPassword);

    /// <summary>
    /// Remueve la autenticación del cliente
    /// </summary>
    protected void RemoveAuthentication()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Obtiene un token JWT para el usuario especificado
    /// </summary>
    private async Task<string> GetJwtTokenAsync(string email, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        return result?.AccessToken ?? throw new InvalidOperationException("No se pudo obtener el token");
    }

    /// <summary>
    /// Deserializa una respuesta ApiResponse
    /// </summary>
    protected async Task<ApiResponse<T>?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
    }

    /// <summary>
    /// Obtiene el ID del usuario Admin de prueba
    /// </summary>
    protected async Task<string> GetAdminUserIdAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync(TestUsers.AdminEmail);
        return admin?.Id ?? throw new InvalidOperationException("Admin no encontrado");
    }

    /// <summary>
    /// Obtiene el ID del usuario Barbero de prueba
    /// </summary>
    protected async Task<string> GetBarberoUserIdAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var barbero = await userManager.FindByEmailAsync(TestUsers.BarberoEmail);
        return barbero?.Id ?? throw new InvalidOperationException("Barbero no encontrado");
    }

    /// <summary>
    /// Obtiene el ID del usuario Cliente de prueba
    /// </summary>
    protected async Task<string> GetClienteUserIdAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var cliente = await userManager.FindByEmailAsync(TestUsers.ClienteEmail);
        return cliente?.Id ?? throw new InvalidOperationException("Cliente no encontrado");
    }

    /// <summary>
    /// Obtiene el DbContext para operaciones directas
    /// </summary>
    protected ApplicationDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}


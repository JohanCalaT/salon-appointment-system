using SalonAppointmentSystem.Web;
using SalonAppointmentSystem.Web.Components;
using SalonAppointmentSystem.Web.Extensions;
using SalonAppointmentSystem.Web.Services.Auth;
using SalonAppointmentSystem.Web.Services.Http;
using SalonAppointmentSystem.Web.Constants;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

// ===== MEDIATR (CQRS) =====
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// ===== BLAZORISE =====
builder.Services.AddBlazoriseCommunity();

// ===== AUTENTICACIÓN Y AUTORIZACIÓN =====
// Configurar políticas de autorización basadas en roles
builder.Services.AddAuthorizationCore(options =>
{
    // Política para Administradores
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole(AppRoles.Admin));

    // Política para Barberos
    options.AddPolicy("RequireBarbero", policy =>
        policy.RequireRole(AppRoles.Barbero));

    // Política para Clientes
    options.AddPolicy("RequireCliente", policy =>
        policy.RequireRole(AppRoles.Cliente));

    // Política para Admin o Barbero
    options.AddPolicy("RequireAdminOrBarbero", policy =>
        policy.RequireRole(AppRoles.Admin, AppRoles.Barbero));

    // Política para cualquier usuario autenticado (Admin, Barbero o Cliente)
    options.AddPolicy("RequireAuthenticated", policy =>
        policy.RequireRole(AppRoles.Admin, AppRoles.Barbero, AppRoles.Cliente));
});

// Habilitar CascadingAuthenticationState para que el estado de autenticación
// esté disponible en toda la jerarquía de componentes
builder.Services.AddCascadingAuthenticationState();

// Registrar servicios de autenticación
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

// Registrar el DelegatingHandler para inyectar tokens automáticamente
builder.Services.AddTransient<AuthTokenHandler>();

// ===== HTTP CLIENTS =====
// Cliente HTTP para operaciones de autenticación (SIN AuthTokenHandler para evitar dependencia circular)
builder.Services.AddHttpClient("AuthClient", client =>
{
    client.BaseAddress = new Uri("https+http://apiservice");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Cliente HTTP para operaciones autenticadas (CON AuthTokenHandler)
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://apiservice");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<AuthTokenHandler>(); // ← Agregar handler para inyectar JWT

// Cliente HTTP de ejemplo (Weather) - puede eliminarse cuando no se necesite
builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();

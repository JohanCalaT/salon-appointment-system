using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SalonAppointmentSystem.Shared.DTOs.Auth;
using SalonAppointmentSystem.Web.Features.Auth.Models;
using SalonAppointmentSystem.Web.Services.Auth;
using SalonAppointmentSystem.Web.Constants;

namespace SalonAppointmentSystem.Web.Features.Auth.Components;

/// <summary>
/// Panel lateral de login que aparece en el Estado 2
/// Ocupa el 40% del ancho en desktop, 100% en mobile
/// Implementa autenticación real con JWT
/// </summary>
public partial class LoginPanel : ComponentBase
{
    // ===================================================================
    // INYECCIÓN DE DEPENDENCIAS
    // ===================================================================

    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<LoginPanel> Logger { get; set; } = default!;

    // ===================================================================
    // PARÁMETROS
    // ===================================================================

    /// <summary>
    /// Callback que se invoca cuando se cierra el panel
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>
    /// URL a la que redirigir después del login exitoso
    /// Si no se especifica, se redirige según el rol del usuario
    /// </summary>
    [Parameter]
    public string? ReturnUrl { get; set; }

    // ===================================================================
    // ESTADO DEL COMPONENTE
    // ===================================================================

    private LoginModel loginModel = new();
    private bool isLoading = false;
    private string? errorMessage = null;

    // ===================================================================
    // CICLO DE VIDA
    // ===================================================================

    protected override async Task OnInitializedAsync()
    {
        // Verificar si el usuario ya está autenticado
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            Logger.LogInformation("Usuario ya autenticado, redirigiendo...");
            await RedirectAfterLogin(user);
        }
    }

    // ===================================================================
    // MÉTODOS DE AUTENTICACIÓN
    // ===================================================================

    /// <summary>
    /// Maneja el submit del formulario de login
    /// </summary>
    private async Task OnLoginSubmitAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = null;
            StateHasChanged();

            Logger.LogInformation("Intentando login para: {Email}", loginModel.Email);

            // Crear request de login
            var loginRequest = new LoginRequest
            {
                Email = loginModel.Email,
                Password = loginModel.Password
            };

            // Llamar al servicio de autenticación
            var response = await AuthService.LoginAsync(loginRequest);

            if (!response.Success || string.IsNullOrEmpty(response.AccessToken))
            {
                errorMessage = response.Message ?? "Credenciales inválidas. Por favor, verifica tu email y contraseña.";
                Logger.LogWarning("Login fallido para {Email}: {Message}", loginModel.Email, errorMessage);
                return;
            }

            Logger.LogInformation("Login exitoso para {Email}, Rol: {Role}",
                loginModel.Email, response.User?.Rol);

            // Notificar al AuthenticationStateProvider que el usuario se autenticó
            if (AuthStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                customProvider.NotifyUserAuthentication();
            }

            // Cerrar el panel de login
            await OnClose.InvokeAsync();

            // Redirigir según el rol del usuario
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            await RedirectAfterLogin(authState.User);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error durante el login para {Email}", loginModel.Email);
            errorMessage = "Error de conexión. Por favor, intenta nuevamente.";
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Redirige al usuario según su rol después de un login exitoso
    /// </summary>
    private async Task RedirectAfterLogin(System.Security.Claims.ClaimsPrincipal user)
    {
        // Si hay ReturnUrl, redirigir ahí
        if (!string.IsNullOrEmpty(ReturnUrl))
        {
            Navigation.NavigateTo(ReturnUrl);
            return;
        }

        // Redirigir según el rol
        if (user.IsInRole(AppRoles.Admin) || user.IsInRole(AppRoles.Barbero))
        {
            Logger.LogInformation("Redirigiendo a Dashboard");
            Navigation.NavigateTo(AppRoutes.Dashboard);
        }
        else if (user.IsInRole(AppRoles.Cliente))
        {
            Logger.LogInformation("Redirigiendo a Reservas");
            Navigation.NavigateTo(AppRoutes.Reservas);
        }
        else
        {
            // Rol desconocido, ir a Home
            Logger.LogWarning("Rol desconocido, redirigiendo a Home");
            Navigation.NavigateTo(AppRoutes.Home);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Maneja el clic en el botón de cerrar (X)
    /// </summary>
    private async Task OnCloseClicked()
    {
        if (!isLoading)
        {
            await OnClose.InvokeAsync();
        }
    }
}


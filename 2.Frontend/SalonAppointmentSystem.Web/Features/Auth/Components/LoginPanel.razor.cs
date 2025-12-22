using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SalonAppointmentSystem.Shared.DTOs.Auth;
using SalonAppointmentSystem.Web.Features.Auth.Models;
using SalonAppointmentSystem.Web.Services.Auth;
using SalonAppointmentSystem.Web.Constants;

namespace SalonAppointmentSystem.Web.Features.Auth.Components;

/// <summary>
/// Panel lateral de login con autenticación JWT
/// </summary>
public partial class LoginPanel : ComponentBase
{
    #region Inyección de Dependencias

    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<LoginPanel> Logger { get; set; } = default!;

    #endregion

    #region Parámetros

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Parameter]
    public string? ReturnUrl { get; set; }

    #endregion

    #region Estado del Componente

    private LoginModel loginModel = new();
    private bool isLoading;
    private string? errorMessage;

    #endregion

    #region Ciclo de Vida

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            Logger.LogInformation("Usuario ya autenticado, redirigiendo...");
            await RedirectAfterLogin(user);
        }
    }

    #endregion

    #region Métodos de Autenticación

    private async Task OnLoginSubmitAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = null;

            Logger.LogInformation("Intentando login para: {Email}", loginModel.Email);

            var loginRequest = new LoginRequest
            {
                Email = loginModel.Email,
                Password = loginModel.Password
            };

            var response = await AuthService.LoginAsync(loginRequest);

            if (!response.Success || string.IsNullOrEmpty(response.AccessToken))
            {
                errorMessage = response.Message ?? "Credenciales inválidas. Por favor, verifica tu email y contraseña.";
                Logger.LogWarning("Login fallido para {Email}: {Message}", loginModel.Email, errorMessage);
                return;
            }

            Logger.LogInformation("Login exitoso para {Email}, Rol: {Role}", loginModel.Email, response.User?.Rol);

            if (AuthStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                customProvider.NotifyUserAuthentication();
            }

            await OnClose.InvokeAsync();

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
        }
    }

    private async Task RedirectAfterLogin(System.Security.Claims.ClaimsPrincipal user)
    {
        if (!string.IsNullOrEmpty(ReturnUrl))
        {
            Navigation.NavigateTo(ReturnUrl);
            return;
        }

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
            Logger.LogWarning("Rol desconocido, redirigiendo a Home");
            Navigation.NavigateTo(AppRoutes.Home);
        }

        await Task.CompletedTask;
    }

    private async Task OnCloseClicked()
    {
        if (!isLoading)
        {
            await OnClose.InvokeAsync();
        }
    }

    #endregion
}

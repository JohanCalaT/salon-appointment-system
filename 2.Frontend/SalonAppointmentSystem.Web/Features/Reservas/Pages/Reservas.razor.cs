using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SalonAppointmentSystem.Web.Constants;

namespace SalonAppointmentSystem.Web.Features.Reservas.Pages;

/// <summary>
/// Página de Reservas accesible para todos los usuarios
/// </summary>
public partial class Reservas : ComponentBase
{
    #region Inyección de Dependencias

    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<Reservas> Logger { get; set; } = default!;

    #endregion

    #region Estado del Componente

    private bool isAuthenticated;
    private string? userName;
    private string? userRole;
    private bool isLoading = true;
    private bool hasRendered;

    #endregion

    #region Ciclo de Vida

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            hasRendered = true;
            await LoadUserInfoAsync();
            isLoading = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Métodos Privados

    private async Task LoadUserInfoAsync()
    {
        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            isAuthenticated = user.Identity?.IsAuthenticated ?? false;

            if (isAuthenticated)
            {
                userName = user.FindFirst(AppClaimTypes.FullName)?.Value
                          ?? user.FindFirst(AppClaimTypes.Email)?.Value
                          ?? "Usuario";
                userRole = user.FindFirst(AppClaimTypes.Role)?.Value ?? "Usuario";

                Logger.LogInformation("Usuario autenticado accediendo a Reservas: {UserName}, Rol: {Role}",
                    userName, userRole);
            }
            else
            {
                Logger.LogInformation("Usuario invitado accediendo a Reservas");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al cargar información del usuario");
        }
    }

    private void OnLoginClicked()
    {
        Logger.LogInformation("Invitado solicitando login desde Reservas");
        Navigation.NavigateTo($"{AppRoutes.Home}?showLogin=true");
    }

    private void OnRegisterClicked()
    {
        Logger.LogInformation("Invitado solicitando registro desde Reservas");
        Navigation.NavigateTo($"{AppRoutes.Home}?showRegister=true");
    }

    #endregion
}

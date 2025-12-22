using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using SalonAppointmentSystem.Web.Constants;

namespace SalonAppointmentSystem.Web.Components.Pages;

/// <summary>
/// Página de Acceso Denegado (403)
/// </summary>
public partial class AccessDenied : ComponentBase
{
    #region Inyección de Dependencias

    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<AccessDenied> Logger { get; set; } = default!;

    #endregion

    #region Estado del Componente

    private string? userName;
    private string? userRole;

    #endregion

    #region Ciclo de Vida

    protected override async Task OnInitializedAsync()
    {
        await LoadUserInfoAsync();
    }

    #endregion

    #region Métodos Privados

    private async Task LoadUserInfoAsync()
    {
        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                userName = user.FindFirst(AppClaimTypes.FullName)?.Value
                          ?? user.FindFirst(AppClaimTypes.Email)?.Value
                          ?? "Usuario";
                userRole = user.FindFirst(AppClaimTypes.Role)?.Value ?? "Usuario";

                Logger.LogWarning("Usuario {UserName} con rol {Role} intentó acceder a recurso no autorizado",
                    userName, userRole);
            }
            else
            {
                Logger.LogWarning("Usuario no autenticado intentó acceder a recurso protegido");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al cargar información del usuario en AccessDenied");
        }
    }

    private void OnGoHomeClicked()
    {
        Logger.LogInformation("Usuario navegando a Home desde AccessDenied");
        Navigation.NavigateTo(AppRoutes.Home);
    }

    private async Task OnGoBackClicked()
    {
        try
        {
            Logger.LogInformation("Usuario volviendo atrás desde AccessDenied");
            await JSRuntime.InvokeVoidAsync("history.back");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al volver atrás");
            Navigation.NavigateTo(AppRoutes.Home);
        }
    }

    private void OnLoginClicked()
    {
        Logger.LogInformation("Usuario no autenticado solicitando login desde AccessDenied");
        var returnUrl = Navigation.Uri;
        Navigation.NavigateTo($"{AppRoutes.Home}?returnUrl={Uri.EscapeDataString(returnUrl)}");
    }

    #endregion
}

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SalonAppointmentSystem.Web.Services.Auth;
using SalonAppointmentSystem.Web.Constants;
using Blazorise;

namespace SalonAppointmentSystem.Web.Features.Auth.Components;

/// <summary>
/// Componente de botón para cerrar sesión
/// </summary>
public partial class LogoutButton : ComponentBase
{
    #region Inyección de Dependencias

    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<LogoutButton> Logger { get; set; } = default!;

    #endregion

    #region Parámetros

    [Parameter]
    public string ButtonText { get; set; } = "Cerrar Sesión";

    [Parameter]
    public Color ButtonColor { get; set; } = Color.Danger;

    [Parameter]
    public bool Outline { get; set; }

    [Parameter]
    public Size ButtonSize { get; set; } = Size.Default;

    [Parameter]
    public bool ShowIcon { get; set; } = true;

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public bool RequireConfirmation { get; set; }

    [Parameter]
    public string RedirectUrl { get; set; } = AppRoutes.Home;

    [Parameter]
    public EventCallback OnLogoutCompleted { get; set; }

    #endregion

    #region Estado del Componente

    private bool isLoading;

    #endregion

    #region Métodos

    private async Task OnLogoutClickedAsync()
    {
        try
        {
            isLoading = true;

            Logger.LogInformation("Iniciando logout");

            await AuthService.LogoutAsync();

            if (AuthStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                customProvider.NotifyUserLogout();
            }

            Logger.LogInformation("Logout exitoso, redirigiendo a {RedirectUrl}", RedirectUrl);

            if (OnLogoutCompleted.HasDelegate)
            {
                await OnLogoutCompleted.InvokeAsync();
            }

            Navigation.NavigateTo(RedirectUrl, forceLoad: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error durante el logout");
            Navigation.NavigateTo(AppRoutes.Home, forceLoad: true);
        }
        finally
        {
            isLoading = false;
        }
    }

    #endregion
}

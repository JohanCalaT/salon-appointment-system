using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SalonAppointmentSystem.Web.Services.Auth;
using SalonAppointmentSystem.Web.Constants;
using Blazorise;

namespace SalonAppointmentSystem.Web.Features.Auth.Components;

/// <summary>
/// Componente de botón para cerrar sesión
/// Maneja el logout y redirige al usuario a la página de inicio
/// </summary>
public partial class LogoutButton : ComponentBase
{
    // ===================================================================
    // INYECCIÓN DE DEPENDENCIAS
    // ===================================================================

    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<LogoutButton> Logger { get; set; } = default!;

    // ===================================================================
    // PARÁMETROS
    // ===================================================================

    /// <summary>
    /// Texto del botón
    /// </summary>
    [Parameter]
    public string ButtonText { get; set; } = "Cerrar Sesión";

    /// <summary>
    /// Color del botón (Blazorise)
    /// </summary>
    [Parameter]
    public Color ButtonColor { get; set; } = Color.Danger;

    /// <summary>
    /// Si el botón debe ser outline
    /// </summary>
    [Parameter]
    public bool Outline { get; set; } = false;

    /// <summary>
    /// Tamaño del botón
    /// </summary>
    [Parameter]
    public Size ButtonSize { get; set; } = Size.Default;

    /// <summary>
    /// Mostrar icono de logout
    /// </summary>
    [Parameter]
    public bool ShowIcon { get; set; } = true;

    /// <summary>
    /// Clases CSS adicionales
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Requiere confirmación antes de cerrar sesión
    /// </summary>
    [Parameter]
    public bool RequireConfirmation { get; set; } = false;

    /// <summary>
    /// URL a la que redirigir después del logout
    /// Por defecto redirige a Home
    /// </summary>
    [Parameter]
    public string RedirectUrl { get; set; } = AppRoutes.Home;

    /// <summary>
    /// Callback que se invoca después de un logout exitoso
    /// </summary>
    [Parameter]
    public EventCallback OnLogoutCompleted { get; set; }

    // ===================================================================
    // ESTADO DEL COMPONENTE
    // ===================================================================

    private bool isLoading = false;

    // ===================================================================
    // MÉTODOS
    // ===================================================================

    /// <summary>
    /// Maneja el clic en el botón de logout
    /// </summary>
    private async Task OnLogoutClickedAsync()
    {
        try
        {
            // Si requiere confirmación, mostrar diálogo (por ahora sin confirmación)
            // TODO: Implementar modal de confirmación si RequireConfirmation = true

            isLoading = true;
            StateHasChanged();

            Logger.LogInformation("Iniciando logout");

            // Llamar al servicio de autenticación para cerrar sesión
            await AuthService.LogoutAsync();

            // Notificar al AuthenticationStateProvider que el usuario cerró sesión
            if (AuthStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                customProvider.NotifyUserLogout();
            }

            Logger.LogInformation("Logout exitoso, redirigiendo a {RedirectUrl}", RedirectUrl);

            // Invocar callback si existe
            if (OnLogoutCompleted.HasDelegate)
            {
                await OnLogoutCompleted.InvokeAsync();
            }

            // Redirigir a la URL especificada
            Navigation.NavigateTo(RedirectUrl, forceLoad: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error durante el logout");
            // Aún así redirigir a Home en caso de error
            Navigation.NavigateTo(AppRoutes.Home, forceLoad: true);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
}


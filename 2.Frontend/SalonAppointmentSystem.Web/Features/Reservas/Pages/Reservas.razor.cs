using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SalonAppointmentSystem.Web.Constants;

namespace SalonAppointmentSystem.Web.Features.Reservas.Pages;

/// <summary>
/// Página de Reservas accesible para todos los usuarios
/// Muestra mensaje de bienvenida diferente para usuarios autenticados e invitados
/// </summary>
public partial class Reservas : ComponentBase
{
    // ===================================================================
    // INYECCIÓN DE DEPENDENCIAS
    // ===================================================================

    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<Reservas> Logger { get; set; } = default!;

    // ===================================================================
    // ESTADO DEL COMPONENTE
    // ===================================================================

    private bool isAuthenticated = false;
    private string? userName;
    private string? userRole;
    private bool isLoading = true;
    private bool hasRendered = false;

    // ===================================================================
    // CICLO DE VIDA
    // ===================================================================

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

    // ===================================================================
    // MÉTODOS PRIVADOS
    // ===================================================================

    /// <summary>
    /// Carga la información del usuario si está autenticado
    /// </summary>
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

    // ===================================================================
    // MANEJADORES DE EVENTOS
    // ===================================================================

    /// <summary>
    /// Maneja el clic en el enlace de login
    /// </summary>
    private void OnLoginClicked()
    {
        Logger.LogInformation("Invitado solicitando login desde Reservas");
        // TODO: Abrir panel de login o navegar a página de login
        // Por ahora navegamos a Home donde está el panel de login
        Navigation.NavigateTo($"{AppRoutes.Home}?showLogin=true");
    }

    /// <summary>
    /// Maneja el clic en el enlace de registro
    /// </summary>
    private void OnRegisterClicked()
    {
        Logger.LogInformation("Invitado solicitando registro desde Reservas");
        // TODO: Abrir panel de registro o navegar a página de registro
        // Por ahora navegamos a Home
        Navigation.NavigateTo($"{AppRoutes.Home}?showRegister=true");
    }
}


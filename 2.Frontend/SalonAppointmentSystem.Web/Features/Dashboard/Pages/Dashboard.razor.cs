using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SalonAppointmentSystem.Web.Constants;

namespace SalonAppointmentSystem.Web.Features.Dashboard.Pages;

/// <summary>
/// Página de Dashboard protegida para Admin y Barbero
/// </summary>
public partial class Dashboard : ComponentBase
{
    #region Inyección de Dependencias

    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private ILogger<Dashboard> Logger { get; set; } = default!;

    #endregion

    #region Estado del Componente

    private int citasHoy;
    private int clientesActivos;
    private decimal ingresosMes;
    private string? userRole;
    private bool isLoading = true;
    private bool isAuthorized;
    private bool hasRendered;

    #endregion

    #region Ciclo de Vida

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            hasRendered = true;

            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            Logger.LogInformation("Dashboard - Usuario autenticado: {IsAuth}, Roles: {Roles}",
                user.Identity?.IsAuthenticated,
                string.Join(", ", user.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)));

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                Logger.LogWarning("Usuario no autenticado intentando acceder a Dashboard");
                Navigation.NavigateTo(AppRoutes.Home, forceLoad: true);
                return;
            }

            if (!user.IsInRole(AppRoles.Admin) && !user.IsInRole(AppRoles.Barbero))
            {
                Logger.LogWarning("Usuario {UserId} sin permisos intentando acceder a Dashboard",
                    user.FindFirst(AppClaimTypes.UserId)?.Value);
                Navigation.NavigateTo(AppRoutes.AccessDenied, forceLoad: true);
                return;
            }

            isAuthorized = true;
            await LoadDashboardDataAsync();
            isLoading = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Métodos Privados

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            userRole = user.FindFirst(AppClaimTypes.Role)?.Value;

            Logger.LogInformation("Cargando dashboard para usuario con rol: {Role}", userRole);

            citasHoy = 12;
            clientesActivos = 156;
            ingresosMes = 15750.50m;

            Logger.LogDebug("Datos del dashboard cargados: Citas={Citas}, Clientes={Clientes}, Ingresos={Ingresos}",
                citasHoy, clientesActivos, ingresosMes);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al cargar datos del dashboard");
        }
    }

    private void OnNuevaCitaClicked()
    {
        Logger.LogInformation("Navegando a nueva cita");
        Navigation.NavigateTo("/citas/nueva");
    }

    private void OnVerReservasClicked()
    {
        Logger.LogInformation("Navegando a reservas");
        Navigation.NavigateTo(AppRoutes.Reservas);
    }

    private void OnGestionarUsuariosClicked()
    {
        Logger.LogInformation("Navegando a gestión de usuarios");
        Navigation.NavigateTo("/admin/usuarios");
    }

    private void OnConfiguracionClicked()
    {
        Logger.LogInformation("Navegando a configuración");
        Navigation.NavigateTo("/admin/configuracion");
    }

    #endregion
}

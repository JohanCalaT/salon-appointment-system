using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SalonAppointmentSystem.Web.Constants;

namespace SalonAppointmentSystem.Web.Features.Dashboard.Pages;

/// <summary>
/// Página de Dashboard protegida para Admin y Barbero
/// Muestra estadísticas y acciones rápidas
/// </summary>
public partial class Dashboard : ComponentBase
{
    // ===================================================================
    // INYECCIÓN DE DEPENDENCIAS
    // ===================================================================

    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private ILogger<Dashboard> Logger { get; set; } = default!;

    // ===================================================================
    // ESTADO DEL COMPONENTE
    // ===================================================================

    private int citasHoy = 0;
    private int clientesActivos = 0;
    private decimal ingresosMes = 0;
    private string? userRole;
    private bool isLoading = true;
    private bool isAuthorized = false;
    private bool hasRendered = false;

    // ===================================================================
    // CICLO DE VIDA
    // ===================================================================

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            hasRendered = true;

            // Verificar autorización después del primer render
            // Esto asegura que ProtectedSessionStorage esté disponible
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

            // Verificar que sea Admin o Barbero
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

    // ===================================================================
    // MÉTODOS PRIVADOS
    // ===================================================================

    /// <summary>
    /// Carga los datos del dashboard
    /// Por ahora son datos de ejemplo, después se conectarán a la API
    /// </summary>
    private async Task LoadDashboardDataAsync()
    {
        try
        {
            // Obtener rol del usuario
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            userRole = user.FindFirst(AppClaimTypes.Role)?.Value;

            Logger.LogInformation("Cargando dashboard para usuario con rol: {Role}", userRole);

            // TODO: Reemplazar con llamadas reales a la API
            // Por ahora usamos datos de ejemplo
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

    // ===================================================================
    // MANEJADORES DE EVENTOS
    // ===================================================================

    private void OnNuevaCitaClicked()
    {
        Logger.LogInformation("Navegando a nueva cita");
        // TODO: Navegar a página de nueva cita cuando esté implementada
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
        // TODO: Navegar a página de gestión de usuarios cuando esté implementada
        Navigation.NavigateTo("/admin/usuarios");
    }

    private void OnConfiguracionClicked()
    {
        Logger.LogInformation("Navegando a configuración");
        // TODO: Navegar a página de configuración cuando esté implementada
        Navigation.NavigateTo("/admin/configuracion");
    }
}


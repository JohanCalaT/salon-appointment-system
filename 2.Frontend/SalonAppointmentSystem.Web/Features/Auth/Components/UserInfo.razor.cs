using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SalonAppointmentSystem.Web.Services.Auth;
using SalonAppointmentSystem.Web.Constants;
using Blazorise;

namespace SalonAppointmentSystem.Web.Features.Auth.Components;

/// <summary>
/// Componente para mostrar información del usuario autenticado
/// Extrae datos del AuthenticationState y los muestra de forma personalizable
/// </summary>
public partial class UserInfo : ComponentBase
{
    // ===================================================================
    // INYECCIÓN DE DEPENDENCIAS
    // ===================================================================

    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private ILogger<UserInfo> Logger { get; set; } = default!;

    // ===================================================================
    // PARÁMETROS
    // ===================================================================

    /// <summary>
    /// Mostrar avatar del usuario
    /// </summary>
    [Parameter]
    public bool ShowAvatar { get; set; } = true;

    /// <summary>
    /// Mostrar nombre del usuario
    /// </summary>
    [Parameter]
    public bool ShowName { get; set; } = true;

    /// <summary>
    /// Mostrar email del usuario
    /// </summary>
    [Parameter]
    public bool ShowEmail { get; set; } = false;

    /// <summary>
    /// Mostrar rol del usuario
    /// </summary>
    [Parameter]
    public bool ShowRole { get; set; } = true;

    /// <summary>
    /// Mostrar puntos acumulados
    /// </summary>
    [Parameter]
    public bool ShowPoints { get; set; } = false;

    /// <summary>
    /// Mostrar mensaje de invitado cuando no está autenticado
    /// </summary>
    [Parameter]
    public bool ShowGuestMessage { get; set; } = true;

    /// <summary>
    /// Clases CSS adicionales
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    // ===================================================================
    // ESTADO DEL COMPONENTE
    // ===================================================================

    private string? userName;
    private string? userEmail;
    private string? userRole;
    private int userPoints;

    // ===================================================================
    // CICLO DE VIDA
    // ===================================================================

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadUserInfoAsync();
            StateHasChanged();
        }
    }

    // ===================================================================
    // MÉTODOS PRIVADOS
    // ===================================================================

    /// <summary>
    /// Carga la información del usuario desde el AuthenticationState
    /// </summary>
    private async Task LoadUserInfoAsync()
    {
        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                // Obtener información del usuario desde el AuthService
                var userInfo = await AuthService.GetCurrentUserInfoAsync();

                if (userInfo != null)
                {
                    userName = userInfo.NombreCompleto;
                    userEmail = userInfo.Email;
                    userRole = userInfo.Rol;
                    userPoints = userInfo.PuntosAcumulados;
                }
                else
                {
                    // Fallback: extraer desde claims
                    userName = user.FindFirst(AppClaimTypes.FullName)?.Value 
                              ?? user.FindFirst(AppClaimTypes.Email)?.Value 
                              ?? "Usuario";
                    userEmail = user.FindFirst(AppClaimTypes.Email)?.Value;
                    userRole = user.FindFirst(AppClaimTypes.Role)?.Value;
                    
                    var pointsClaim = user.FindFirst(AppClaimTypes.Puntos)?.Value;
                    userPoints = int.TryParse(pointsClaim, out var points) ? points : 0;
                }

                Logger.LogDebug("Información de usuario cargada: {UserName}, Rol: {Role}", userName, userRole);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al cargar información del usuario");
        }
    }

    /// <summary>
    /// Obtiene el color del badge según el rol
    /// </summary>
    private Color GetRoleBadgeColor(string role)
    {
        return role switch
        {
            AppRoles.Admin => Color.Danger,
            AppRoles.Barbero => Color.Primary,
            AppRoles.Cliente => Color.Success,
            AppRoles.Invitado => Color.Secondary,
            _ => Color.Secondary
        };
    }

    /// <summary>
    /// Obtiene el nombre de visualización del rol
    /// </summary>
    private string GetRoleDisplayName(string role)
    {
        return role switch
        {
            AppRoles.Admin => "Administrador",
            AppRoles.Barbero => "Barbero",
            AppRoles.Cliente => "Cliente",
            AppRoles.Invitado => "Invitado",
            _ => role
        };
    }
}


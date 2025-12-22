using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SalonAppointmentSystem.Web.Services.Auth;
using SalonAppointmentSystem.Web.Constants;
using Blazorise;

namespace SalonAppointmentSystem.Web.Features.Auth.Components;

/// <summary>
/// Componente para mostrar información del usuario autenticado
/// </summary>
public partial class UserInfo : ComponentBase
{
    #region Inyección de Dependencias

    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private ILogger<UserInfo> Logger { get; set; } = default!;

    #endregion

    #region Parámetros

    [Parameter]
    public bool ShowAvatar { get; set; } = true;

    [Parameter]
    public bool ShowName { get; set; } = true;

    [Parameter]
    public bool ShowEmail { get; set; }

    [Parameter]
    public bool ShowRole { get; set; } = true;

    [Parameter]
    public bool ShowPoints { get; set; }

    [Parameter]
    public bool ShowGuestMessage { get; set; } = true;

    [Parameter]
    public string? CssClass { get; set; }

    #endregion

    #region Estado del Componente

    private string? userName;
    private string? userEmail;
    private string? userRole;
    private int userPoints;

    #endregion

    #region Ciclo de Vida

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadUserInfoAsync();
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

            if (user.Identity?.IsAuthenticated == true)
            {
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

    #endregion
}

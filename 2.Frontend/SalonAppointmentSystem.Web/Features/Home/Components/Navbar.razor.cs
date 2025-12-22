using Microsoft.AspNetCore.Components;

namespace SalonAppointmentSystem.Web.Features.Home.Components;

/// <summary>
/// Navbar transparente para la página principal
/// </summary>
public partial class Navbar : ComponentBase
{
    #region Inyección de Dependencias

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    #endregion

    #region Parámetros

    [Parameter]
    public EventCallback OnLoginClicked { get; set; }

    #endregion

    #region Métodos

    private void OnReservarClicked()
    {
        NavigationManager.NavigateTo("/reservas");
    }

    private async Task HandleLoginClicked()
    {
        await OnLoginClicked.InvokeAsync();
    }

    #endregion
}

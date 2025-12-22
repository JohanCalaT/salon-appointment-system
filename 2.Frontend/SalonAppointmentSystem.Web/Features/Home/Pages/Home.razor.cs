using Microsoft.AspNetCore.Components;

namespace SalonAppointmentSystem.Web.Features.Home.Pages;

/// <summary>
/// Página principal con Hero inmersivo y panel de login
/// </summary>
public partial class Home : ComponentBase
{
    #region Inyección de Dependencias

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    #endregion

    #region Estado del Componente

    private bool ShowLoginPanel { get; set; }

    #endregion

    #region Métodos

    private void ToggleLoginPanel()
    {
        ShowLoginPanel = !ShowLoginPanel;
    }

    private void OnReservarClicked()
    {
        NavigationManager.NavigateTo("/reservas");
    }

    #endregion
}

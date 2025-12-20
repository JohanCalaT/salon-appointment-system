using Microsoft.AspNetCore.Components;

namespace SalonAppointmentSystem.Web.Features.Home.Pages;

/// <summary>
/// Página principal con Hero inmersivo y panel de login
/// Estado 1: Hero completo (100vh)
/// Estado 2: Split screen con panel de login (60% Hero + 40% Login)
/// </summary>
public partial class Home : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Controla la visibilidad del panel de login
    /// </summary>
    private bool ShowLoginPanel { get; set; } = false;

    /// <summary>
    /// Alterna la visibilidad del panel de login
    /// Transición suave entre Estado 1 y Estado 2
    /// </summary>
    private void ToggleLoginPanel()
    {
        ShowLoginPanel = !ShowLoginPanel;
        // StateHasChanged() no es necesario aquí - Blazor lo hace automáticamente
    }

    /// <summary>
    /// Maneja el clic en el botón "Reservar Cita"
    /// </summary>
    private void OnReservarClicked()
    {
        // TODO: Navegar a la página de reservas cuando esté implementada
        // NavigationManager.NavigateTo("/reservar");
        
        // Por ahora, solo un log en consola
        Console.WriteLine("Botón 'Reservar Cita' clickeado");
    }
}


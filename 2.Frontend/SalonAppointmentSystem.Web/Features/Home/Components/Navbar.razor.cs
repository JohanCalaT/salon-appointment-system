using Microsoft.AspNetCore.Components;

namespace SalonAppointmentSystem.Web.Features.Home.Components;

/// <summary>
/// Navbar transparente para la página principal
/// Comunica eventos al componente padre (Home) mediante EventCallback
/// </summary>
public partial class Navbar : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// EventCallback que se invoca cuando se hace clic en el botón "Login"
    /// Permite comunicación con el componente padre (Home)
    /// </summary>
    [Parameter]
    public EventCallback OnLoginClicked { get; set; }

    /// <summary>
    /// Maneja el clic en el botón "Reservar"
    /// </summary>
    private void OnReservarClicked()
    {
        // TODO: Navegar a la página de reservas
        Console.WriteLine("Navbar: Botón 'Reservar' clickeado");
    }

    /// <summary>
    /// Maneja el clic en el botón "Login" y notifica al padre
    /// </summary>
    private async Task HandleLoginClicked()
    {
        Console.WriteLine("Navbar: Botón 'Login' clickeado - Notificando al padre");
        await OnLoginClicked.InvokeAsync();
    }
}


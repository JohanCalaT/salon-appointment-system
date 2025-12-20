using Microsoft.AspNetCore.Components;

namespace SalonAppointmentSystem.Web.Features.Auth.Components;

/// <summary>
/// Panel lateral de login que aparece en el Estado 2
/// Ocupa el 40% del ancho en desktop, 100% en mobile
/// </summary>
public partial class LoginPanel : ComponentBase
{
    /// <summary>
    /// Callback que se invoca cuando se cierra el panel
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    // ===================================================================
    // ESTADO DEL FORMULARIO (Solo visual por ahora)
    // ===================================================================

    private string email = string.Empty;
    private string password = string.Empty;
    private bool rememberMe = false;

    /// <summary>
    /// Maneja el clic en el botón de cerrar (X)
    /// </summary>
    private async Task OnCloseClicked()
    {
        await OnClose.InvokeAsync();
    }

    /// <summary>
    /// Maneja el clic en el botón "Iniciar Sesión"
    /// Por ahora solo visual, la funcionalidad se implementará después
    /// </summary>
    private void OnLoginClicked()
    {
        // TODO: Implementar lógica de autenticación
        Console.WriteLine($"Login clickeado - Email: {email}");
    }
}


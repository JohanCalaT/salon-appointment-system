using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Application.DTOs.Reservas;

/// <summary>
/// DTO de respuesta con información completa de una reserva
/// </summary>
public record ReservaDto
{
    public int Id { get; init; }
    public string CodigoReserva { get; init; } = string.Empty;
    
    // Información de la estación
    public int EstacionId { get; init; }
    public string EstacionNombre { get; init; } = string.Empty;
    public string? BarberoNombre { get; init; }
    
    // Información del servicio
    public int ServicioId { get; init; }
    public string ServicioNombre { get; init; } = string.Empty;
    
    // Información del cliente
    public string? UsuarioId { get; init; }
    public string NombreCliente { get; init; } = string.Empty;
    public string EmailCliente { get; init; } = string.Empty;
    public string TelefonoCliente { get; init; } = string.Empty;
    
    // Información de la cita
    public DateTime FechaHora { get; init; }
    public DateTime FechaHoraFin { get; init; }
    public int DuracionMinutos { get; init; }
    public decimal Precio { get; init; }
    public int PuntosGanados { get; init; }
    
    // Estado y tipo
    public EstadoReserva Estado { get; init; }
    public string EstadoNombre => Estado.ToString();
    public TipoReserva TipoReserva { get; init; }
    public string TipoReservaNombre => TipoReserva.ToString();
    
    // Auditoría
    public DateTime FechaCreacion { get; init; }
    public string? CreadaPor { get; init; }
    public string? RolCreador { get; init; }
    
    // Cancelación (si aplica)
    public DateTime? FechaCancelacion { get; init; }
    public string? CanceladaPor { get; init; }
    public string? MotivoCancelacion { get; init; }
    
    // Propiedades calculadas
    public bool EsFutura => FechaHora > DateTime.UtcNow;
    public bool PuedeSerCancelada => Estado != EstadoReserva.Cancelada && 
                                      Estado != EstadoReserva.Completada && 
                                      EsFutura;
}

/// <summary>
/// DTO simplificado para listados
/// </summary>
public record ReservaListDto
{
    public int Id { get; init; }
    public string CodigoReserva { get; init; } = string.Empty;
    public string EstacionNombre { get; init; } = string.Empty;
    public string ServicioNombre { get; init; } = string.Empty;
    public string NombreCliente { get; init; } = string.Empty;
    public DateTime FechaHora { get; init; }
    public int DuracionMinutos { get; init; }
    public decimal Precio { get; init; }
    public EstadoReserva Estado { get; init; }
    public string EstadoNombre => Estado.ToString();
}


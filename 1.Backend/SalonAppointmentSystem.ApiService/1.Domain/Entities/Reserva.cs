using SalonAppointmentSystem.ApiService.Domain.Common;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Domain.Entities;

/// <summary>
/// Entidad central que representa una reserva/cita en la barbería
/// </summary>
public class Reserva : BaseEntity
{
    // Caracteres permitidos para código de reserva (sin I, O, 0, 1 para evitar confusión)
    private static readonly char[] CodigoChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    /// <summary>
    /// Código único de 8 caracteres para que invitados consulten su reserva
    /// Ejemplo: "ABC12345"
    /// </summary>
    public string CodigoReserva { get; set; } = GenerarCodigoReserva();

    /// <summary>
    /// FK a la estación donde se realizará el servicio
    /// </summary>
    public int EstacionId { get; set; }

    /// <summary>
    /// FK al usuario registrado (null para invitados)
    /// </summary>
    public string? UsuarioId { get; set; }

    /// <summary>
    /// ID del usuario que creó la reserva (Admin, Barbero, Cliente, o null para invitado self-service)
    /// </summary>
    public string? CreadaPor { get; set; }

    /// <summary>
    /// Rol del usuario que creó la reserva (para auditoría y permisos)
    /// </summary>
    public string? RolCreador { get; set; }

    /// <summary>
    /// Nombre del cliente (requerido para invitados y usuarios)
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;

    /// <summary>
    /// Email del cliente para contacto
    /// </summary>
    public string EmailCliente { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono del cliente para contacto
    /// </summary>
    public string TelefonoCliente { get; set; } = string.Empty;

    /// <summary>
    /// FK al servicio solicitado
    /// </summary>
    public int ServicioId { get; set; }

    /// <summary>
    /// Fecha y hora de la reserva
    /// </summary>
    public DateTime FechaHora { get; set; }

    /// <summary>
    /// Duración en minutos (denormalizado del servicio)
    /// </summary>
    public int DuracionMinutos { get; set; }

    /// <summary>
    /// Estado actual de la reserva
    /// </summary>
    public EstadoReserva Estado { get; set; } = EstadoReserva.Pendiente;

    /// <summary>
    /// Tipo de reserva (Manual o CitaRapida)
    /// </summary>
    public TipoReserva TipoReserva { get; set; } = TipoReserva.Manual;

    /// <summary>
    /// Puntos ganados por esta reserva (al completarse)
    /// </summary>
    public int PuntosGanados { get; set; }

    /// <summary>
    /// Precio del servicio (denormalizado del servicio)
    /// </summary>
    public decimal Precio { get; set; }

    /// <summary>
    /// Fecha de creación de la reserva
    /// </summary>
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha de cancelación (si aplica)
    /// </summary>
    public DateTime? FechaCancelacion { get; set; }

    /// <summary>
    /// ID del usuario que canceló o "System"
    /// </summary>
    public string? CanceladaPor { get; set; }

    /// <summary>
    /// Motivo de la cancelación
    /// </summary>
    public string? MotivoCancelacion { get; set; }

    /// <summary>
    /// Genera un código único de 8 caracteres alfanuméricos
    /// </summary>
    private static string GenerarCodigoReserva()
    {
        var random = Random.Shared;
        return new string(Enumerable.Range(0, 8)
            .Select(_ => CodigoChars[random.Next(CodigoChars.Length)])
            .ToArray());
    }

    // Propiedades de navegación (virtual para lazy loading)
    public virtual Estacion Estacion { get; set; } = null!;
    public virtual Servicio Servicio { get; set; } = null!;

    // Métodos de dominio
    /// <summary>
    /// Calcula la hora de finalización de la reserva
    /// </summary>
    public DateTime FechaHoraFin => FechaHora.AddMinutes(DuracionMinutos);

    /// <summary>
    /// Verifica si hay solapamiento con otra reserva
    /// </summary>
    public bool TieneSolapamientoCon(DateTime otraFechaHora, int otraDuracion)
    {
        var otraFechaHoraFin = otraFechaHora.AddMinutes(otraDuracion);
        return FechaHora < otraFechaHoraFin && FechaHoraFin > otraFechaHora;
    }

    /// <summary>
    /// Cancela la reserva
    /// </summary>
    public void Cancelar(string canceladaPor, string? motivo = null)
    {
        Estado = EstadoReserva.Cancelada;
        FechaCancelacion = DateTime.UtcNow;
        CanceladaPor = canceladaPor;
        MotivoCancelacion = motivo;
    }

    /// <summary>
    /// Confirma la reserva
    /// </summary>
    public void Confirmar()
    {
        Estado = EstadoReserva.Confirmada;
    }

    /// <summary>
    /// Completa la reserva
    /// </summary>
    public void Completar()
    {
        Estado = EstadoReserva.Completada;
    }
}


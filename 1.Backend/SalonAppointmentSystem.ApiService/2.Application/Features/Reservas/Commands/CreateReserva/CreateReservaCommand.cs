using MediatR;
using SalonAppointmentSystem.ApiService.Application.Common;
using SalonAppointmentSystem.Shared.DTOs.Reservas;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Application.Features.Reservas.Commands.CreateReserva;

/// <summary>
/// Command para crear una nueva reserva
/// </summary>
public record CreateReservaCommand : IRequest<Result<ReservaDto>>
{
    /// <summary>
    /// ID de la estación donde se realizará el servicio
    /// </summary>
    public int EstacionId { get; init; }
    
    /// <summary>
    /// ID del servicio a realizar
    /// </summary>
    public int ServicioId { get; init; }
    
    /// <summary>
    /// Fecha y hora de la reserva (UTC)
    /// </summary>
    public DateTime FechaHora { get; init; }
    
    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string NombreCliente { get; init; } = string.Empty;
    
    /// <summary>
    /// Email del cliente
    /// </summary>
    public string EmailCliente { get; init; } = string.Empty;
    
    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string TelefonoCliente { get; init; } = string.Empty;
    
    /// <summary>
    /// ID del usuario registrado (null para invitados)
    /// </summary>
    public string? UsuarioId { get; init; }
    
    /// <summary>
    /// ID del usuario que crea la reserva (Admin/Barbero/Cliente, null para invitado self-service)
    /// </summary>
    public string? CreadaPor { get; init; }
    
    /// <summary>
    /// Rol del usuario que crea la reserva
    /// </summary>
    public string? RolCreador { get; init; }
    
    /// <summary>
    /// Tipo de reserva (Manual o CitaRapida)
    /// </summary>
    public TipoReserva TipoReserva { get; init; } = TipoReserva.Manual;
    
    /// <summary>
    /// Crea un comando desde el request del controller
    /// </summary>
    public static CreateReservaCommand FromRequest(
        CreateReservaRequest request,
        string? usuarioId,
        string? creadaPor,
        string? rolCreador)
    {
        return new CreateReservaCommand
        {
            EstacionId = request.EstacionId,
            ServicioId = request.ServicioId,
            FechaHora = request.FechaHora,
            NombreCliente = request.NombreCliente,
            EmailCliente = request.EmailCliente,
            TelefonoCliente = request.TelefonoCliente,
            UsuarioId = usuarioId,
            CreadaPor = creadaPor,
            RolCreador = rolCreador,
            TipoReserva = request.TipoReserva
        };
    }
}


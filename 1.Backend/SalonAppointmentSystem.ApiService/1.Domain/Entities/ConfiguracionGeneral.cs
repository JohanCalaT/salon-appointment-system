using SalonAppointmentSystem.ApiService.Domain.Common;
using SalonAppointmentSystem.Shared.Enums;

namespace SalonAppointmentSystem.ApiService.Domain.Entities;

/// <summary>
/// Parámetros de configuración del sistema
/// </summary>
public class ConfiguracionGeneral : BaseEntity
{
    /// <summary>
    /// Clave única de la configuración
    /// </summary>
    public string Clave { get; set; } = string.Empty;

    /// <summary>
    /// Valor de la configuración (almacenado como string)
    /// </summary>
    public string Valor { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la configuración
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Tipo de dato del valor
    /// </summary>
    public TipoDatoConfig TipoDato { get; set; }

    // Métodos de dominio para obtener el valor tipado
    public int ObtenerValorEntero() => int.TryParse(Valor, out var result) ? result : 0;
    
    public decimal ObtenerValorDecimal() => decimal.TryParse(Valor, out var result) ? result : 0;
    
    public bool ObtenerValorBoolean() => bool.TryParse(Valor, out var result) && result;
    
    public string ObtenerValorTexto() => Valor;
}

/// <summary>
/// Claves de configuración predefinidas del sistema
/// </summary>
public static class ConfiguracionClaves
{
    public const string TiempoMinimoAnticipacionMinutos = "TiempoMinimoAnticipacionMinutos";
    public const string DiasMaximoReservaFutura = "DiasMaximoReservaFutura";
    public const string PermitirReservasInvitados = "PermitirReservasInvitados";
    public const string PuntosRequeridosDescuento = "PuntosRequeridosDescuento";
}


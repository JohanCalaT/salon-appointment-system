namespace SalonAppointmentSystem.ApiService.Application.Common.Constants;

/// <summary>
/// Tipos de claims personalizados para la aplicación
/// </summary>
public static class AppClaimTypes
{
    /// <summary>
    /// ID del usuario
    /// </summary>
    public const string UserId = "uid";

    /// <summary>
    /// Email del usuario
    /// </summary>
    public const string Email = "email";

    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public const string FullName = "full_name";

    /// <summary>
    /// Rol del usuario
    /// </summary>
    public const string Role = "role";

    /// <summary>
    /// ID de la estación asignada (para barberos)
    /// </summary>
    public const string EstacionId = "estacion_id";

    /// <summary>
    /// Puntos acumulados del usuario
    /// </summary>
    public const string Puntos = "puntos";

    /// <summary>
    /// Indica si el usuario está activo
    /// </summary>
    public const string IsActive = "is_active";
}

/// <summary>
/// Políticas de autorización de la aplicación
/// </summary>
public static class AppPolicies
{
    /// <summary>
    /// Solo administradores
    /// </summary>
    public const string RequireAdmin = "RequireAdmin";

    /// <summary>
    /// Administradores y barberos
    /// </summary>
    public const string RequireAdminOrBarbero = "RequireAdminOrBarbero";

    /// <summary>
    /// Cualquier usuario autenticado (Cliente, Barbero o Admin)
    /// </summary>
    public const string RequireAuthenticated = "RequireAuthenticated";

    /// <summary>
    /// Solo clientes
    /// </summary>
    public const string RequireCliente = "RequireCliente";

    /// <summary>
    /// Solo barberos
    /// </summary>
    public const string RequireBarbero = "RequireBarbero";
}


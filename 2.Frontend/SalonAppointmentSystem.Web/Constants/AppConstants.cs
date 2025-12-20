namespace SalonAppointmentSystem.Web.Constants;

/// <summary>
/// Roles del sistema - Deben coincidir con los roles del backend
/// </summary>
public static class AppRoles
{
    /// <summary>
    /// Rol de Administrador - Acceso completo al sistema
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Rol de Barbero - Gestión de reservas y servicios
    /// </summary>
    public const string Barbero = "Barbero";

    /// <summary>
    /// Rol de Cliente - Creación y gestión de sus propias reservas
    /// </summary>
    public const string Cliente = "Cliente";

    /// <summary>
    /// Rol de Invitado - Acceso limitado sin autenticación
    /// </summary>
    public const string Invitado = "Invitado";

    /// <summary>
    /// Todos los roles del sistema
    /// </summary>
    public static readonly string[] TodosLosRoles = { Admin, Barbero, Cliente, Invitado };
}

/// <summary>
/// Tipos de claims personalizados - Deben coincidir con los del backend
/// Estos claims se extraen del JWT token
/// </summary>
public static class AppClaimTypes
{
    /// <summary>
    /// ID del usuario (uid)
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
    /// Rol del usuario (Admin, Barbero, Cliente, Invitado)
    /// </summary>
    public const string Role = "role";

    /// <summary>
    /// ID de la estación asignada (solo para barberos)
    /// </summary>
    public const string EstacionId = "estacion_id";

    /// <summary>
    /// Puntos de fidelidad acumulados
    /// </summary>
    public const string Puntos = "puntos";

    /// <summary>
    /// Indica si el usuario está activo
    /// </summary>
    public const string IsActive = "is_active";
}

/// <summary>
/// Claves para almacenamiento en ProtectedSessionStorage
/// IMPORTANTE: En Blazor Server, los tokens se almacenan en el servidor, NO en el navegador
/// </summary>
public static class StorageKeys
{
    /// <summary>
    /// Clave para almacenar el Access Token JWT
    /// </summary>
    public const string AccessToken = "auth_access_token";

    /// <summary>
    /// Clave para almacenar el Refresh Token
    /// </summary>
    public const string RefreshToken = "auth_refresh_token";

    /// <summary>
    /// Clave para almacenar la fecha de expiración del token
    /// </summary>
    public const string TokenExpiration = "auth_token_expiration";

    /// <summary>
    /// Clave para almacenar información del usuario
    /// </summary>
    public const string UserInfo = "auth_user_info";
}

/// <summary>
/// Rutas de la aplicación
/// </summary>
public static class AppRoutes
{
    /// <summary>
    /// Página principal
    /// </summary>
    public const string Home = "/";

    /// <summary>
    /// Dashboard para Admin y Barbero
    /// </summary>
    public const string Dashboard = "/dashboard";

    /// <summary>
    /// Página de reservas (Cliente e Invitado)
    /// </summary>
    public const string Reservas = "/reservas";

    /// <summary>
    /// Página de acceso denegado
    /// </summary>
    public const string AccessDenied = "/access-denied";
}

/// <summary>
/// Endpoints de la API
/// </summary>
public static class ApiEndpoints
{
    /// <summary>
    /// Base para endpoints de autenticación
    /// </summary>
    public const string AuthBase = "api/auth";

    /// <summary>
    /// Login de usuario
    /// </summary>
    public const string Login = $"{AuthBase}/login";

    /// <summary>
    /// Registro de nuevo usuario
    /// </summary>
    public const string Register = $"{AuthBase}/register";

    /// <summary>
    /// Renovar access token
    /// </summary>
    public const string RefreshToken = $"{AuthBase}/refresh-token";

    /// <summary>
    /// Cerrar sesión (revocar refresh token)
    /// </summary>
    public const string Logout = $"{AuthBase}/logout";

    /// <summary>
    /// Cerrar todas las sesiones del usuario
    /// </summary>
    public const string LogoutAll = $"{AuthBase}/logout-all";

    /// <summary>
    /// Obtener información del usuario actual
    /// </summary>
    public const string Me = $"{AuthBase}/me";
}


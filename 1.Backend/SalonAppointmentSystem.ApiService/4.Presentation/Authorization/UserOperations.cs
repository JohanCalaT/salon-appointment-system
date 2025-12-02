namespace SalonAppointmentSystem.ApiService.Presentation.Authorization;

/// <summary>
/// Define las operaciones disponibles para recursos de usuarios
/// </summary>
public static class UserOperations
{
    /// <summary>
    /// Lectura de usuarios (lista, detalle)
    /// </summary>
    public const string Read = "Users.Read";

    /// <summary>
    /// Crear nuevos usuarios
    /// </summary>
    public const string Create = "Users.Create";

    /// <summary>
    /// Actualizar usuarios existentes
    /// </summary>
    public const string Update = "Users.Update";

    /// <summary>
    /// Eliminar usuarios
    /// </summary>
    public const string Delete = "Users.Delete";

    /// <summary>
    /// Gestión completa (todas las operaciones)
    /// </summary>
    public const string Manage = "Users.Manage";
}

/// <summary>
/// Define las operaciones disponibles para recursos de reservas
/// </summary>
public static class ReservaOperations
{
    public const string Read = "Reservas.Read";
    public const string Create = "Reservas.Create";
    public const string Update = "Reservas.Update";
    public const string Delete = "Reservas.Delete";
    public const string Manage = "Reservas.Manage";
}

/// <summary>
/// Define las operaciones disponibles para recursos de servicios
/// </summary>
public static class ServicioOperations
{
    public const string Read = "Servicios.Read";
    public const string Create = "Servicios.Create";
    public const string Update = "Servicios.Update";
    public const string Delete = "Servicios.Delete";
    public const string Manage = "Servicios.Manage";
}

/// <summary>
/// Define las operaciones disponibles para recursos de estaciones
/// </summary>
public static class EstacionOperations
{
    public const string Read = "Estaciones.Read";
    public const string Create = "Estaciones.Create";
    public const string Update = "Estaciones.Update";
    public const string Delete = "Estaciones.Delete";
    public const string Manage = "Estaciones.Manage";
}


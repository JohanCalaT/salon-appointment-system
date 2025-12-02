using Microsoft.AspNetCore.Authorization;
using SalonAppointmentSystem.ApiService.Infrastructure.Identity;
using System.Security.Claims;

namespace SalonAppointmentSystem.ApiService.Presentation.Authorization;

/// <summary>
/// Handler de autorización basado en operaciones y roles
/// </summary>
public class OperationAuthorizationHandler : AuthorizationHandler<OperationRequirement>
{
    private readonly ILogger<OperationAuthorizationHandler> _logger;

    // Matriz de permisos: qué roles pueden hacer qué operaciones
    private static readonly Dictionary<string, HashSet<string>> RolePermissions = new()
    {
        // Admin puede hacer todo
        [ApplicationRoles.Admin] = new HashSet<string>
        {
            UserOperations.Read, UserOperations.Create, UserOperations.Update, UserOperations.Delete, UserOperations.Manage,
            ReservaOperations.Read, ReservaOperations.Create, ReservaOperations.Update, ReservaOperations.Delete, ReservaOperations.Manage,
            ServicioOperations.Read, ServicioOperations.Create, ServicioOperations.Update, ServicioOperations.Delete, ServicioOperations.Manage,
            EstacionOperations.Read, EstacionOperations.Create, EstacionOperations.Update, EstacionOperations.Delete, EstacionOperations.Manage
        },
        
        // Barbero: solo lectura de usuarios, gestión de sus reservas
        [ApplicationRoles.Barbero] = new HashSet<string>
        {
            UserOperations.Read,
            ReservaOperations.Read, ReservaOperations.Update, // Solo sus reservas
            ServicioOperations.Read,
            EstacionOperations.Read
        },
        
        // Cliente: solo lectura de servicios y estaciones, gestión de sus reservas
        [ApplicationRoles.Cliente] = new HashSet<string>
        {
            ReservaOperations.Read, ReservaOperations.Create, // Solo sus reservas
            ServicioOperations.Read,
            EstacionOperations.Read
        }
    };

    public OperationAuthorizationHandler(ILogger<OperationAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationRequirement requirement)
    {
        var user = context.User;
        
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogDebug("Usuario no autenticado, denegando acceso a {Operation}", requirement.Operation);
            return Task.CompletedTask;
        }

        // Obtener rol del usuario
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        
        if (string.IsNullOrEmpty(userRole))
        {
            _logger.LogWarning("Usuario autenticado sin rol asignado");
            return Task.CompletedTask;
        }

        // Verificar si el rol tiene el permiso
        if (RolePermissions.TryGetValue(userRole, out var permissions) &&
            permissions.Contains(requirement.Operation))
        {
            _logger.LogDebug("Acceso concedido: {Role} -> {Operation}", userRole, requirement.Operation);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogDebug("Acceso denegado: {Role} no tiene permiso para {Operation}", userRole, requirement.Operation);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Extensiones para registrar las políticas de operaciones
/// </summary>
public static class OperationPolicyExtensions
{
    /// <summary>
    /// Registra el handler de autorización por operaciones
    /// </summary>
    public static IServiceCollection AddOperationAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, OperationAuthorizationHandler>();
        return services;
    }

    /// <summary>
    /// Agrega políticas de autorización basadas en operaciones
    /// </summary>
    public static AuthorizationBuilder AddOperationPolicies(this AuthorizationBuilder builder)
    {
        // Políticas para Usuarios
        builder.AddPolicy(UserOperations.Read, policy =>
            policy.Requirements.Add(new OperationRequirement(UserOperations.Read)));
        builder.AddPolicy(UserOperations.Create, policy =>
            policy.Requirements.Add(new OperationRequirement(UserOperations.Create)));
        builder.AddPolicy(UserOperations.Update, policy =>
            policy.Requirements.Add(new OperationRequirement(UserOperations.Update)));
        builder.AddPolicy(UserOperations.Delete, policy =>
            policy.Requirements.Add(new OperationRequirement(UserOperations.Delete)));
        builder.AddPolicy(UserOperations.Manage, policy =>
            policy.Requirements.Add(new OperationRequirement(UserOperations.Manage)));

        // Políticas para Reservas
        builder.AddPolicy(ReservaOperations.Read, policy =>
            policy.Requirements.Add(new OperationRequirement(ReservaOperations.Read)));
        builder.AddPolicy(ReservaOperations.Create, policy =>
            policy.Requirements.Add(new OperationRequirement(ReservaOperations.Create)));
        builder.AddPolicy(ReservaOperations.Update, policy =>
            policy.Requirements.Add(new OperationRequirement(ReservaOperations.Update)));
        builder.AddPolicy(ReservaOperations.Delete, policy =>
            policy.Requirements.Add(new OperationRequirement(ReservaOperations.Delete)));
        builder.AddPolicy(ReservaOperations.Manage, policy =>
            policy.Requirements.Add(new OperationRequirement(ReservaOperations.Manage)));

        // Políticas para Servicios
        builder.AddPolicy(ServicioOperations.Read, policy =>
            policy.Requirements.Add(new OperationRequirement(ServicioOperations.Read)));
        builder.AddPolicy(ServicioOperations.Manage, policy =>
            policy.Requirements.Add(new OperationRequirement(ServicioOperations.Manage)));

        // Políticas para Estaciones
        builder.AddPolicy(EstacionOperations.Read, policy =>
            policy.Requirements.Add(new OperationRequirement(EstacionOperations.Read)));
        builder.AddPolicy(EstacionOperations.Manage, policy =>
            policy.Requirements.Add(new OperationRequirement(EstacionOperations.Manage)));

        return builder;
    }
}


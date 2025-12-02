using Microsoft.AspNetCore.Authorization;

namespace SalonAppointmentSystem.ApiService.Presentation.Authorization;

/// <summary>
/// Requirement para autorización basada en operaciones
/// </summary>
public class OperationRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Nombre de la operación requerida
    /// </summary>
    public string Operation { get; }

    public OperationRequirement(string operation)
    {
        Operation = operation;
    }
}


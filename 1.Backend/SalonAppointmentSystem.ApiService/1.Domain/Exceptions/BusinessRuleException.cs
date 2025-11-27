namespace SalonAppointmentSystem.ApiService.Domain.Exceptions;

/// <summary>
/// Excepción para violaciones de reglas de negocio
/// </summary>
public class BusinessRuleException : DomainException
{
    public string? RuleCode { get; }

    public BusinessRuleException()
        : base()
    {
    }

    public BusinessRuleException(string message)
        : base(message)
    {
    }

    public BusinessRuleException(string message, string ruleCode)
        : base(message)
    {
        RuleCode = ruleCode;
    }

    public BusinessRuleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}


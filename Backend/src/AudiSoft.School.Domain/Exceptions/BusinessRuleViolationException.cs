namespace AudiSoft.School.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando se violan reglas de negocio específicas del dominio.
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public string RuleName { get; }
    public string EntityName { get; }

    public BusinessRuleViolationException(string ruleName, string message)
        : base($"Regla de negocio violada - {ruleName}: {message}", "BUSINESS_RULE_VIOLATION")
    {
        RuleName = ruleName;
        EntityName = string.Empty;
        this.WithContext("RuleName", ruleName);
    }

    public BusinessRuleViolationException(string ruleName, string entityName, string message)
        : base($"Regla de negocio violada en {entityName} - {ruleName}: {message}", "BUSINESS_RULE_VIOLATION")
    {
        RuleName = ruleName;
        EntityName = entityName;
        this.WithContext("RuleName", ruleName)
            .WithContext("EntityName", entityName);
    }

    public BusinessRuleViolationException(string ruleName, string entityName, object entityId, string message)
        : base($"Regla de negocio violada en {entityName} (ID: {entityId}) - {ruleName}: {message}", "BUSINESS_RULE_VIOLATION")
    {
        RuleName = ruleName;
        EntityName = entityName;
        this.WithContext("RuleName", ruleName)
            .WithContext("EntityName", entityName)
            .WithContext("EntityId", entityId);
    }
}
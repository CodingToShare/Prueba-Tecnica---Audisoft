namespace AudiSoft.School.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando la entidad se encuentra en un estado inválido o reglas de negocio son violadas.
/// </summary>
public class InvalidEntityStateException : DomainException
{
    public string? EntityName { get; }
    public string? PropertyName { get; }

    public InvalidEntityStateException(string message) 
        : base(message, "INVALID_ENTITY_STATE") { }

    public InvalidEntityStateException(string entityName, string propertyName, string message) 
        : base($"Estado inválido en {entityName}.{propertyName}: {message}", "INVALID_ENTITY_STATE")
    {
        EntityName = entityName;
        PropertyName = propertyName;
        this.WithContext("EntityName", entityName)
            .WithContext("PropertyName", propertyName);
    }

    public InvalidEntityStateException(string message, Exception innerException) 
        : base(message, innerException, "INVALID_ENTITY_STATE") { }

    public InvalidEntityStateException(string entityName, string message, Dictionary<string, object> validationContext) 
        : base($"Validación fallida en {entityName}: {message}", "VALIDATION_FAILED")
    {
        EntityName = entityName;
        this.WithContext("EntityName", entityName);
        
        foreach (var item in validationContext)
        {
            this.WithContext(item.Key, item.Value);
        }
    }
}

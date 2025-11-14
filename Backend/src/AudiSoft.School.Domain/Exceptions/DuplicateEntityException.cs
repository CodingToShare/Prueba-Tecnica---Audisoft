namespace AudiSoft.School.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando se intenta crear una entidad que ya existe.
/// </summary>
public class DuplicateEntityException : DomainException
{
    public string EntityName { get; }
    public string ConflictingProperty { get; }
    public object ConflictingValue { get; }

    public DuplicateEntityException(string entityName, string conflictingProperty, object conflictingValue)
        : base($"Ya existe un {entityName} con {conflictingProperty} = '{conflictingValue}'", "DUPLICATE_ENTITY")
    {
        EntityName = entityName;
        ConflictingProperty = conflictingProperty;
        ConflictingValue = conflictingValue;
        
        this.WithContext("EntityName", entityName)
            .WithContext("ConflictingProperty", conflictingProperty)
            .WithContext("ConflictingValue", conflictingValue);
    }

    public DuplicateEntityException(string entityName, string conflictingProperty, object conflictingValue, string customMessage)
        : base(customMessage, "DUPLICATE_ENTITY")
    {
        EntityName = entityName;
        ConflictingProperty = conflictingProperty;
        ConflictingValue = conflictingValue;
        
        this.WithContext("EntityName", entityName)
            .WithContext("ConflictingProperty", conflictingProperty)
            .WithContext("ConflictingValue", conflictingValue);
    }
}
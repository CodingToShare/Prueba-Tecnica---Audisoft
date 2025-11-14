namespace AudiSoft.School.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando una entidad no es encontrada.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityName, int id)
        : base($"{entityName} con ID {id} no fue encontrado", "ENTITY_NOT_FOUND")
    {
        EntityName = entityName;
        EntityId = id;
        this.WithContext("EntityName", entityName)
            .WithContext("EntityId", id);
    }

    public EntityNotFoundException(string entityName, string id)
        : base($"{entityName} con ID '{id}' no fue encontrado", "ENTITY_NOT_FOUND")
    {
        EntityName = entityName;
        EntityId = id;
        this.WithContext("EntityName", entityName)
            .WithContext("EntityId", id);
    }

    public EntityNotFoundException(string entityName, object id, string customMessage)
        : base(customMessage, "ENTITY_NOT_FOUND")
    {
        EntityName = entityName;
        EntityId = id;
        this.WithContext("EntityName", entityName)
            .WithContext("EntityId", id);
    }
}

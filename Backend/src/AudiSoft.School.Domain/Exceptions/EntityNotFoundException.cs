namespace AudiSoft.School.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando una entidad no es encontrada.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, int id)
        : base($"{entityName} con ID {id} no fue encontrado")
    {
        EntityName = entityName;
        EntityId = id;
    }

    public string EntityName { get; }
    public int EntityId { get; }
}

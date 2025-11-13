namespace AudiSoft.School.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando la entidad se encuentra en un estado inválido.
/// </summary>
public class InvalidEntityStateException : DomainException
{
    public InvalidEntityStateException(string message) : base(message) { }
}

namespace AudiSoft.School.Domain.Exceptions;

/// <summary>
/// Excepción base para errores de dominio.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

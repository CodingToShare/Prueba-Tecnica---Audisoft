namespace AudiSoft.School.Domain.Exceptions;

/// <summary>
/// Excepción base para errores de dominio.
/// Proporciona propiedades adicionales para contexto y rastreo.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Código de error específico del dominio.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Contexto adicional del error (entidad, operación, etc.).
    /// </summary>
    public Dictionary<string, object> Context { get; }

    protected DomainException(string message, string errorCode = "DOMAIN_ERROR") : base(message) 
    { 
        ErrorCode = errorCode;
        Context = new Dictionary<string, object>();
    }

    protected DomainException(string message, Exception innerException, string errorCode = "DOMAIN_ERROR") 
        : base(message, innerException) 
    { 
        ErrorCode = errorCode;
        Context = new Dictionary<string, object>();
    }

    /// <summary>
    /// Agrega contexto adicional al error.
    /// </summary>
    public DomainException WithContext(string key, object value)
    {
        Context[key] = value;
        return this;
    }
}

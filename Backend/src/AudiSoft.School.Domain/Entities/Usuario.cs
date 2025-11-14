namespace AudiSoft.School.Domain.Entities;

/// <summary>
/// Entidad Usuario para autenticación y autorización.
/// </summary>
public class Usuario : BaseEntity
{
    /// <summary>
    /// Nombre de usuario único para login.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Hash de la contraseña (nunca almacenar contraseñas en texto plano).
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Referencia opcional al Profesor asociado (para usuarios tipo Profesor).
    /// </summary>
    public int? IdProfesor { get; set; }

    /// <summary>
    /// Referencia opcional al Estudiante asociado (para usuarios tipo Estudiante).
    /// </summary>
    public int? IdEstudiante { get; set; }

    /// <summary>
    /// Email del usuario (opcional, para futuras funcionalidades).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Indica si el usuario está activo.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Fecha del último login.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    // Propiedades de navegación
    /// <summary>
    /// Profesor asociado (si aplica).
    /// </summary>
    public virtual Profesor? Profesor { get; set; }

    /// <summary>
    /// Estudiante asociado (si aplica).
    /// </summary>
    public virtual Estudiante? Estudiante { get; set; }

    /// <summary>
    /// Roles asignados al usuario.
    /// </summary>
    public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
}
namespace AudiSoft.School.Domain.Entities;

/// <summary>
/// Entidad Rol para definir permisos y accesos.
/// </summary>
public class Rol : BaseEntity
{
    /// <summary>
    /// Nombre del rol (Admin, Profesor, Estudiante).
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del rol y sus permisos.
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Indica si el rol está activo.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Propiedades de navegación
    /// <summary>
    /// Usuarios que tienen este rol asignado.
    /// </summary>
    public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
}

/// <summary>
/// Constantes para los nombres de roles del sistema.
/// </summary>
public static class RolNames
{
    public const string Admin = "Admin";
    public const string Profesor = "Profesor";
    public const string Estudiante = "Estudiante";
}
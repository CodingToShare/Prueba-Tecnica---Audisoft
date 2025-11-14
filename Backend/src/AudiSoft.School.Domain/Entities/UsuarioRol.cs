namespace AudiSoft.School.Domain.Entities;

/// <summary>
/// Entidad de relación muchos-a-muchos entre Usuario y Rol.
/// </summary>
public class UsuarioRol : BaseEntity
{
    /// <summary>
    /// ID del usuario.
    /// </summary>
    public int IdUsuario { get; set; }

    /// <summary>
    /// ID del rol.
    /// </summary>
    public int IdRol { get; set; }

    /// <summary>
    /// Fecha desde cuando el usuario tiene este rol asignado.
    /// </summary>
    public DateTime AsignadoEn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha hasta cuando el rol está activo (nullable = sin límite).
    /// </summary>
    public DateTime? ValidoHasta { get; set; }

    /// <summary>
    /// Usuario que asignó este rol.
    /// </summary>
    public string? AsignadoPor { get; set; }

    // Propiedades de navegación
    /// <summary>
    /// Usuario asociado.
    /// </summary>
    public virtual Usuario Usuario { get; set; } = null!;

    /// <summary>
    /// Rol asociado.
    /// </summary>
    public virtual Rol Rol { get; set; } = null!;
}
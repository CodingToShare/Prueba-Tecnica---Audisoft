using System.ComponentModel.DataAnnotations;

namespace AudiSoft.School.Application.DTOs;

/// <summary>
/// DTO para transferencia de datos de Rol.
/// </summary>
public class RolDto
{
    /// <summary>
    /// Identificador único del rol
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre del rol (ej: Admin, Profesor, Estudiante)
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del rol y sus permisos
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Indica si el rol está activo en el sistema
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Número de usuarios que tienen asignado este rol
    /// </summary>
    public int UsuarioCount { get; set; }

    /// <summary>
    /// Fecha de creación del rol
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para crear un nuevo Rol.
/// </summary>
public class CreateRolDto
{
    /// <summary>
    /// Nombre del rol. Solo letras y espacios permitidos.
    /// </summary>
    [Required(ErrorMessage = "El nombre del rol es obligatorio")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre del rol debe tener entre 3 y 50 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El nombre del rol solo puede contener letras y espacios")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del rol y sus permisos (opcional)
    /// </summary>
    [StringLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres")]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Indica si el rol debe estar activo al crearse
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO para actualizar un Rol existente.
/// </summary>
public class UpdateRolDto
{
    /// <summary>
    /// Nuevo nombre del rol. Solo letras y espacios permitidos.
    /// </summary>
    [Required(ErrorMessage = "El nombre del rol es obligatorio")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre del rol debe tener entre 3 y 50 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El nombre del rol solo puede contener letras y espacios")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Nueva descripción del rol
    /// </summary>
    [StringLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres")]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Nuevo estado activo del rol
    /// </summary>
    public bool IsActive { get; set; }
}
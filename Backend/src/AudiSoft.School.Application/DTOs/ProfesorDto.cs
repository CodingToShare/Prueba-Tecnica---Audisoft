using System.ComponentModel.DataAnnotations;

namespace AudiSoft.School.Application.DTOs;

/// <summary>
/// DTO para crear/actualizar un Profesor.
/// </summary>
public class CreateProfesorDto
{
    /// <summary>
    /// Nombre del profesor. Requerido, 3-255 caracteres, solo letras y espacios.
    /// </summary>
    [Required(ErrorMessage = "El nombre del profesor es obligatorio")]
    [StringLength(255, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 255 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
    public string Nombre { get; set; } = null!;
}

/// <summary>
/// DTO para retornar un Profesor en respuestas API.
/// </summary>
public class ProfesorDto
{
    /// <summary>
    /// Identificador único del profesor.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre del profesor.
    /// </summary>
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Fecha de creación del registro.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Usuario que creó el registro.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Fecha de última actualización del registro.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Usuario que actualizó el registro por última vez.
    /// </summary>
    public string? UpdatedBy { get; set; }
}

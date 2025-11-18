using System.ComponentModel.DataAnnotations;

namespace AudiSoft.School.Application.DTOs;

/// <summary>
/// DTO para crear/actualizar una Nota.
/// </summary>
public class CreateNotaDto
{
    /// <summary>
    /// Nombre o descripción de la nota.
    /// </summary>
    [Required(ErrorMessage = "El nombre de la nota es obligatorio")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 200 caracteres")]
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Valor numérico de la nota (0-100, máximo 2 decimales).
    /// </summary>
    [Required(ErrorMessage = "El valor de la nota es obligatorio")]
    [Range(0, 100, ErrorMessage = "El valor de la nota debe estar entre 0 y 100")]
    [RegularExpression(@"^\d{1,2}(\.\d{1,2})?$|^100(\.00?)?$", ErrorMessage = "El valor debe tener máximo 2 decimales")]
    public decimal Valor { get; set; }

    /// <summary>
    /// Identificador del profesor que asigna la nota.
    /// </summary>
    [Required(ErrorMessage = "El ID del profesor es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del profesor debe ser un número positivo")]
    public int IdProfesor { get; set; }

    /// <summary>
    /// Identificador del estudiante que recibe la nota.
    /// </summary>
    [Required(ErrorMessage = "El ID del estudiante es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del estudiante debe ser un número positivo")]
    public int IdEstudiante { get; set; }
}

/// <summary>
/// DTO para retornar una Nota en respuestas API.
/// </summary>
public class NotaDto
{
    /// <summary>
    /// Identificador único de la nota.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre o descripción de la nota.
    /// </summary>
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Valor numérico de la nota.
    /// </summary>
    public decimal Valor { get; set; }

    /// <summary>
    /// Identificador del profesor que asignó la nota.
    /// </summary>
    public int IdProfesor { get; set; }

    /// <summary>
    /// Nombre del profesor que asignó la nota.
    /// </summary>
    public string? NombreProfesor { get; set; }

    /// <summary>
    /// Identificador del estudiante que recibe la nota.
    /// </summary>
    public int IdEstudiante { get; set; }

    /// <summary>
    /// Nombre del estudiante que recibe la nota.
    /// </summary>
    public string? NombreEstudiante { get; set; }

    /// <summary>
    /// Grado del estudiante.
    /// </summary>
    public string? Grado { get; set; }

    /// <summary>
    /// Materia de la nota.
    /// </summary>
    public string? Materia { get; set; }

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

/// <summary>
/// DTO para actualizar una Nota.
/// Separar Create/Update ayuda a mantener contratos claros.
/// </summary>
public class UpdateNotaDto
{
    [Required(ErrorMessage = "El nombre de la nota es obligatorio")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 200 caracteres")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El valor de la nota es obligatorio")]
    [Range(0, 100, ErrorMessage = "El valor de la nota debe estar entre 0 y 100")]
    [RegularExpression(@"^\d{1,2}(\.\d{1,2})?$|^100(\.00?)?$", ErrorMessage = "El valor debe tener máximo 2 decimales")]
    public decimal Valor { get; set; }

    [Required(ErrorMessage = "El ID del profesor es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del profesor debe ser un número positivo")]
    public int IdProfesor { get; set; }

    [Required(ErrorMessage = "El ID del estudiante es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del estudiante debe ser un número positivo")]
    public int IdEstudiante { get; set; }
}

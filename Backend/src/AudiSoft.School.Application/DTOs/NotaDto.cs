namespace AudiSoft.School.Application.DTOs;

/// <summary>
/// DTO para crear/actualizar una Nota.
/// </summary>
public class CreateNotaDto
{
    /// <summary>
    /// Nombre o descripción de la nota.
    /// </summary>
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Valor numérico de la nota (0-100, máximo 2 decimales).
    /// </summary>
    public decimal Valor { get; set; }

    /// <summary>
    /// Identificador del profesor que asigna la nota.
    /// </summary>
    public int IdProfesor { get; set; }

    /// <summary>
    /// Identificador del estudiante que recibe la nota.
    /// </summary>
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
    /// Identificador del estudiante que recibe la nota.
    /// </summary>
    public int IdEstudiante { get; set; }

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

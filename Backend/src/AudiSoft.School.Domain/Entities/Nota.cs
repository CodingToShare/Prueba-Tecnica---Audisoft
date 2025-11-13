namespace AudiSoft.School.Domain.Entities;

/// <summary>
/// Entidad Nota.
/// Relaciones: Profesor 1-N, Estudiante 1-N
/// </summary>
public class Nota : BaseEntity
{
    public string Nombre { get; set; } = null!;

    public decimal Valor { get; set; }

    // Llaves foráneas
    public int IdProfesor { get; set; }
    public int IdEstudiante { get; set; }

    // Navegaciones
    public Profesor Profesor { get; set; } = null!;
    public Estudiante Estudiante { get; set; } = null!;
}

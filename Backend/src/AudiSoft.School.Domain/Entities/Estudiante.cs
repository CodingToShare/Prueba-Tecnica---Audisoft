namespace AudiSoft.School.Domain.Entities;

/// <summary>
/// Entidad Estudiante.
/// </summary>
public class Estudiante : BaseEntity
{
    public string Nombre { get; set; } = null!;

    // Relación: un Estudiante puede tener muchas Notas
    public ICollection<Nota> Notas { get; set; } = new List<Nota>();
}

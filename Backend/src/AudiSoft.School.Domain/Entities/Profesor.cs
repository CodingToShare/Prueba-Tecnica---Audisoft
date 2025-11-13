namespace AudiSoft.School.Domain.Entities;

/// <summary>
/// Entidad Profesor.
/// </summary>
public class Profesor : BaseEntity
{
    public string Nombre { get; set; } = null!;

    // Relación: un Profesor puede tener muchas Notas
    public ICollection<Nota> Notas { get; set; } = new List<Nota>();
}

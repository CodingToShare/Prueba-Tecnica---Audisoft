using AudiSoft.School.Domain.Entities;

namespace AudiSoft.School.Application.Interfaces;

/// <summary>
/// Interfaz para el repositorio de Notas.
/// </summary>
public interface INotaRepository : IRepository<Nota>
{
    Task<List<Nota>> GetByProfesorAsync(int idProfesor);
    Task<List<Nota>> GetByEstudianteAsync(int idEstudiante);
    Task<Nota?> GetByProfesorAndEstudianteAsync(int idProfesor, int idEstudiante);
}

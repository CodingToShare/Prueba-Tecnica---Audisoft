using AudiSoft.School.Domain.Entities;

namespace AudiSoft.School.Application.Interfaces;

/// <summary>
/// Interfaz para el repositorio de Estudiantes.
/// </summary>
public interface IEstudianteRepository : IRepository<Estudiante>
{
    Task<Estudiante?> GetByNombreAsync(string nombre);
}

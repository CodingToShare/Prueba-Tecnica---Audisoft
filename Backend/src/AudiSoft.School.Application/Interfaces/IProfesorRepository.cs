using AudiSoft.School.Domain.Entities;

namespace AudiSoft.School.Application.Interfaces;

/// <summary>
/// Interfaz para el repositorio de Profesores.
/// </summary>
public interface IProfesorRepository : IRepository<Profesor>
{
    Task<Profesor?> GetByNombreAsync(string nombre);
}

using System.Linq.Expressions;

namespace AudiSoft.School.Application.Interfaces;

/// <summary>
/// Interfaz genérica para repositorios.
/// Define las operaciones CRUD básicas.
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
public interface IRepository<T> where T : class
{
    // Read
    /// <summary>
    /// Obtiene una entidad por su ID.
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene todas las entidades.
    /// </summary>
    Task<List<T>> GetAllAsync();

    /// <summary>
    /// Encuentra entidades que cumplan con el predicado especificado.
    /// Ejecuta la consulta en la base de datos para mejor rendimiento.
    /// </summary>
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

    // Create
    /// <summary>
    /// Agrega una nueva entidad.
    /// </summary>
    Task<T> AddAsync(T entity);

    // Update
    /// <summary>
    /// Actualiza una entidad existente.
    /// </summary>
    Task<T> UpdateAsync(T entity);

    // Delete
    /// <summary>
    /// Elimina (marca como eliminado) una entidad por su ID.
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Verifica si una entidad existe por su ID.
    /// </summary>
    Task<bool> ExistsAsync(int id);
}

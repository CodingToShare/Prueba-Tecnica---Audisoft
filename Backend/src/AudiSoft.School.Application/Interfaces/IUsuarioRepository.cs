using AudiSoft.School.Domain.Entities;

namespace AudiSoft.School.Application.Interfaces;

/// <summary>
/// Repositorio específico para operaciones de Usuario.
/// </summary>
public interface IUsuarioRepository : IRepository<Usuario>
{
    /// <summary>
    /// Busca un usuario por su nombre de usuario.
    /// </summary>
    /// <param name="userName">Nombre de usuario</param>
    /// <returns>Usuario o null si no existe</returns>
    Task<Usuario?> GetByUserNameAsync(string userName);

    /// <summary>
    /// Busca un usuario por su email.
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <returns>Usuario o null si no existe</returns>
    Task<Usuario?> GetByEmailAsync(string email);

    /// <summary>
    /// Obtiene un usuario con sus roles incluidos.
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Usuario con roles o null si no existe</returns>
    Task<Usuario?> GetWithRolesAsync(int id);

    /// <summary>
    /// Obtiene un usuario por nombre de usuario con sus roles incluidos.
    /// </summary>
    /// <param name="userName">Nombre de usuario</param>
    /// <returns>Usuario con roles o null si no existe</returns>
    Task<Usuario?> GetByUserNameWithRolesAsync(string userName);

    /// <summary>
    /// Obtiene usuarios que están asociados a un profesor específico.
    /// </summary>
    /// <param name="idProfesor">ID del profesor</param>
    /// <returns>Lista de usuarios asociados</returns>
    Task<List<Usuario>> GetByProfesorAsync(int idProfesor);

    /// <summary>
    /// Obtiene usuarios que están asociados a un estudiante específico.
    /// </summary>
    /// <param name="idEstudiante">ID del estudiante</param>
    /// <returns>Lista de usuarios asociados</returns>
    Task<List<Usuario>> GetByEstudianteAsync(int idEstudiante);

    /// <summary>
    /// Verifica si existe un usuario con el nombre de usuario especificado.
    /// </summary>
    /// <param name="userName">Nombre de usuario a verificar</param>
    /// <param name="excludeUserId">ID de usuario a excluir de la verificación (para updates)</param>
    /// <returns>true si existe, false si no existe</returns>
    Task<bool> ExistsByUserNameAsync(string userName, int? excludeUserId = null);
}
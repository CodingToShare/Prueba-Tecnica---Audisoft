using AudiSoft.School.Domain.Entities;

namespace AudiSoft.School.Application.Interfaces;

/// <summary>
/// Repositorio específico para operaciones de Rol.
/// </summary>
public interface IRolRepository : IRepository<Rol>
{
    /// <summary>
    /// Busca un rol por su nombre.
    /// </summary>
    /// <param name="nombre">Nombre del rol</param>
    /// <returns>Rol o null si no existe</returns>
    Task<Rol?> GetByNombreAsync(string nombre);

    /// <summary>
    /// Obtiene todos los roles activos.
    /// </summary>
    /// <returns>Lista de roles activos</returns>
    Task<List<Rol>> GetActiveRolesAsync();

    /// <summary>
    /// Obtiene roles con la cantidad de usuarios asignados.
    /// </summary>
    /// <returns>Lista de roles con conteo de usuarios</returns>
    Task<List<(Rol Rol, int UsuarioCount)>> GetRolesWithUserCountAsync();

    /// <summary>
    /// Verifica si existe un rol con el nombre especificado.
    /// </summary>
    /// <param name="nombre">Nombre del rol a verificar</param>
    /// <param name="excludeRolId">ID de rol a excluir de la verificación (para updates)</param>
    /// <returns>true si existe, false si no existe</returns>
    Task<bool> ExistsByNombreAsync(string nombre, int? excludeRolId = null);
}
using AudiSoft.School.Domain.Entities;

namespace AudiSoft.School.Application.Interfaces;

/// <summary>
/// Repositorio específico para operaciones de UsuarioRol.
/// </summary>
public interface IUsuarioRolRepository : IRepository<UsuarioRol>
{
    /// <summary>
    /// Obtiene los roles de un usuario específico.
    /// </summary>
    /// <param name="idUsuario">ID del usuario</param>
    /// <returns>Lista de asignaciones usuario-rol activas</returns>
    Task<List<UsuarioRol>> GetByUsuarioAsync(int idUsuario);

    /// <summary>
    /// Obtiene los usuarios que tienen un rol específico.
    /// </summary>
    /// <param name="idRol">ID del rol</param>
    /// <returns>Lista de asignaciones usuario-rol activas</returns>
    Task<List<UsuarioRol>> GetByRolAsync(int idRol);

    /// <summary>
    /// Obtiene una asignación específica usuario-rol.
    /// </summary>
    /// <param name="idUsuario">ID del usuario</param>
    /// <param name="idRol">ID del rol</param>
    /// <returns>Asignación usuario-rol o null si no existe</returns>
    Task<UsuarioRol?> GetByUsuarioAndRolAsync(int idUsuario, int idRol);

    /// <summary>
    /// Verifica si un usuario tiene un rol específico asignado.
    /// </summary>
    /// <param name="idUsuario">ID del usuario</param>
    /// <param name="idRol">ID del rol</param>
    /// <returns>true si tiene el rol asignado, false en caso contrario</returns>
    Task<bool> HasRolAsync(int idUsuario, int idRol);

    /// <summary>
    /// Verifica si un usuario tiene un rol específico por nombre.
    /// </summary>
    /// <param name="idUsuario">ID del usuario</param>
    /// <param name="nombreRol">Nombre del rol</param>
    /// <returns>true si tiene el rol asignado, false en caso contrario</returns>
    Task<bool> HasRolByNameAsync(int idUsuario, string nombreRol);

    /// <summary>
    /// Obtiene los nombres de roles de un usuario.
    /// </summary>
    /// <param name="idUsuario">ID del usuario</param>
    /// <returns>Lista de nombres de roles</returns>
    Task<List<string>> GetRoleNamesAsync(int idUsuario);

    /// <summary>
    /// Asigna un rol a un usuario.
    /// </summary>
    /// <param name="idUsuario">ID del usuario</param>
    /// <param name="idRol">ID del rol</param>
    /// <param name="asignadoPor">Usuario que asigna el rol</param>
    /// <param name="validoHasta">Fecha límite de validez (opcional)</param>
    /// <returns>Asignación creada</returns>
    Task<UsuarioRol> AsignarRolAsync(int idUsuario, int idRol, string? asignadoPor = null, DateTime? validoHasta = null);

    /// <summary>
    /// Remueve un rol de un usuario (soft delete).
    /// </summary>
    /// <param name="idUsuario">ID del usuario</param>
    /// <param name="idRol">ID del rol</param>
    /// <returns>true si se removió, false si no existía la asignación</returns>
    Task<bool> RemoverRolAsync(int idUsuario, int idRol);
}
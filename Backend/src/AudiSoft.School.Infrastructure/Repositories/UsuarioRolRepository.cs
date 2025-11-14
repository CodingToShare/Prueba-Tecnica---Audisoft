using AudiSoft.School.Application.Interfaces;
using AudiSoft.School.Domain.Entities;
using AudiSoft.School.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AudiSoft.School.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de UsuarioRol.
/// </summary>
public class UsuarioRolRepository : Repository<UsuarioRol>, IUsuarioRolRepository
{
    public UsuarioRolRepository(AudiSoftSchoolDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene los roles de un usuario específico.
    /// </summary>
    public async Task<List<UsuarioRol>> GetByUsuarioAsync(int idUsuario)
    {
        return await _dbSet
            .Where(ur => ur.IdUsuario == idUsuario)
            .Include(ur => ur.Rol)
            .Where(ur => (ur.ValidoHasta == null || ur.ValidoHasta > DateTime.UtcNow))
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene los usuarios que tienen un rol específico.
    /// </summary>
    public async Task<List<UsuarioRol>> GetByRolAsync(int idRol)
    {
        return await _dbSet
            .Where(ur => ur.IdRol == idRol)
            .Include(ur => ur.Usuario)
            .Where(ur => (ur.ValidoHasta == null || ur.ValidoHasta > DateTime.UtcNow))
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene una asignación específica usuario-rol.
    /// </summary>
    public async Task<UsuarioRol?> GetByUsuarioAndRolAsync(int idUsuario, int idRol)
    {
        return await _dbSet
            .Include(ur => ur.Usuario)
            .Include(ur => ur.Rol)
            .FirstOrDefaultAsync(ur => ur.IdUsuario == idUsuario && ur.IdRol == idRol);
    }

    /// <summary>
    /// Verifica si un usuario tiene un rol específico asignado.
    /// </summary>
    public async Task<bool> HasRolAsync(int idUsuario, int idRol)
    {
        return await _dbSet
            .Where(ur => ur.IdUsuario == idUsuario && ur.IdRol == idRol)
            .Where(ur => (ur.ValidoHasta == null || ur.ValidoHasta > DateTime.UtcNow))
            .AnyAsync();
    }

    /// <summary>
    /// Verifica si un usuario tiene un rol específico por nombre.
    /// </summary>
    public async Task<bool> HasRolByNameAsync(int idUsuario, string nombreRol)
    {
        return await _dbSet
            .Where(ur => ur.IdUsuario == idUsuario)
            .Where(ur => ur.Rol.Nombre == nombreRol)
            .Where(ur => (ur.ValidoHasta == null || ur.ValidoHasta > DateTime.UtcNow))
            .AnyAsync();
    }

    /// <summary>
    /// Obtiene los nombres de roles de un usuario.
    /// </summary>
    public async Task<List<string>> GetRoleNamesAsync(int idUsuario)
    {
        return await _dbSet
            .Where(ur => ur.IdUsuario == idUsuario)
            .Where(ur => (ur.ValidoHasta == null || ur.ValidoHasta > DateTime.UtcNow))
            .Select(ur => ur.Rol.Nombre)
            .ToListAsync();
    }

    /// <summary>
    /// Asigna un rol a un usuario.
    /// </summary>
    public async Task<UsuarioRol> AsignarRolAsync(int idUsuario, int idRol, string? asignadoPor = null, DateTime? validoHasta = null)
    {
        // Verificar si ya existe la asignación
        var existingAssignment = await GetByUsuarioAndRolAsync(idUsuario, idRol);
        if (existingAssignment != null && !existingAssignment.IsDeleted)
        {
            // Si ya existe y está activa, actualizar fechas si es necesario
            if (validoHasta.HasValue)
            {
                existingAssignment.ValidoHasta = validoHasta;
                existingAssignment.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(existingAssignment);
            }
            return existingAssignment;
        }

        // Si existe pero está eliminada, reactivar
        if (existingAssignment != null && existingAssignment.IsDeleted)
        {
            existingAssignment.IsDeleted = false;
            existingAssignment.DeletedAt = null;
            existingAssignment.AsignadoEn = DateTime.UtcNow;
            existingAssignment.ValidoHasta = validoHasta;
            existingAssignment.AsignadoPor = asignadoPor;
            existingAssignment.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(existingAssignment);
            return existingAssignment;
        }

        // Crear nueva asignación
        var usuarioRol = new UsuarioRol
        {
            IdUsuario = idUsuario,
            IdRol = idRol,
            AsignadoEn = DateTime.UtcNow,
            ValidoHasta = validoHasta,
            AsignadoPor = asignadoPor
        };

        return await AddAsync(usuarioRol);
    }

    /// <summary>
    /// Remueve un rol de un usuario (soft delete).
    /// </summary>
    public async Task<bool> RemoverRolAsync(int idUsuario, int idRol)
    {
        var usuarioRol = await GetByUsuarioAndRolAsync(idUsuario, idRol);
        if (usuarioRol == null || usuarioRol.IsDeleted)
        {
            return false;
        }

        usuarioRol.IsDeleted = true;
        usuarioRol.DeletedAt = DateTime.UtcNow;
        usuarioRol.UpdatedAt = DateTime.UtcNow;

        await UpdateAsync(usuarioRol);
        return true;
    }
}
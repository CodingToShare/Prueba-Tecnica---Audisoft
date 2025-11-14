using AudiSoft.School.Application.Interfaces;
using AudiSoft.School.Domain.Entities;
using AudiSoft.School.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AudiSoft.School.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de Rol.
/// </summary>
public class RolRepository : Repository<Rol>, IRolRepository
{
    public RolRepository(AudiSoftSchoolDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Busca un rol por su nombre.
    /// </summary>
    public async Task<Rol?> GetByNombreAsync(string nombre)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.Nombre == nombre);
    }

    /// <summary>
    /// Obtiene todos los roles activos.
    /// </summary>
    public async Task<List<Rol>> GetActiveRolesAsync()
    {
        return await _dbSet
            .Where(r => r.IsActive)
            .OrderBy(r => r.Nombre)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene roles con la cantidad de usuarios asignados.
    /// </summary>
    public async Task<List<(Rol Rol, int UsuarioCount)>> GetRolesWithUserCountAsync()
    {
        return await _dbSet
            .Select(r => new
            {
                Rol = r,
                UsuarioCount = r.UsuarioRoles.Count(ur => !ur.IsDeleted)
            })
            .ToListAsync()
            .ContinueWith(task => task.Result.Select(x => (x.Rol, x.UsuarioCount)).ToList());
    }

    /// <summary>
    /// Verifica si existe un rol con el nombre especificado.
    /// </summary>
    public async Task<bool> ExistsByNombreAsync(string nombre, int? excludeRolId = null)
    {
        var query = _dbSet.Where(r => r.Nombre == nombre);
        
        if (excludeRolId.HasValue)
        {
            query = query.Where(r => r.Id != excludeRolId.Value);
        }

        return await query.AnyAsync();
    }
}
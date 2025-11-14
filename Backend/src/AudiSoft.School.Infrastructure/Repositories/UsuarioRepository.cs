using AudiSoft.School.Application.Interfaces;
using AudiSoft.School.Domain.Entities;
using AudiSoft.School.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AudiSoft.School.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de Usuario.
/// </summary>
public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AudiSoftSchoolDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Busca un usuario por su nombre de usuario.
    /// </summary>
    public async Task<Usuario?> GetByUserNameAsync(string userName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.UserName == userName);
    }

    /// <summary>
    /// Busca un usuario por su email.
    /// </summary>
    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Obtiene un usuario con sus roles incluidos.
    /// </summary>
    public async Task<Usuario?> GetWithRolesAsync(int id)
    {
        return await _dbSet
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .Include(u => u.Profesor)
            .Include(u => u.Estudiante)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// Obtiene un usuario por nombre de usuario con sus roles incluidos.
    /// </summary>
    public async Task<Usuario?> GetByUserNameWithRolesAsync(string userName)
    {
        return await _dbSet
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .Include(u => u.Profesor)
            .Include(u => u.Estudiante)
            .FirstOrDefaultAsync(u => u.UserName == userName);
    }

    /// <summary>
    /// Obtiene usuarios que están asociados a un profesor específico.
    /// </summary>
    public async Task<List<Usuario>> GetByProfesorAsync(int idProfesor)
    {
        return await _dbSet
            .Where(u => u.IdProfesor == idProfesor)
            .Include(u => u.Profesor)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene usuarios que están asociados a un estudiante específico.
    /// </summary>
    public async Task<List<Usuario>> GetByEstudianteAsync(int idEstudiante)
    {
        return await _dbSet
            .Where(u => u.IdEstudiante == idEstudiante)
            .Include(u => u.Estudiante)
            .ToListAsync();
    }

    /// <summary>
    /// Verifica si existe un usuario con el nombre de usuario especificado.
    /// </summary>
    public async Task<bool> ExistsByUserNameAsync(string userName, int? excludeUserId = null)
    {
        var query = _dbSet.Where(u => u.UserName == userName);
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync();
    }
}
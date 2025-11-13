using AudiSoft.School.Application.Interfaces;
using AudiSoft.School.Domain.Entities;
using AudiSoft.School.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AudiSoft.School.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio para Notas.
/// </summary>
public class NotaRepository : Repository<Nota>, INotaRepository
{
    public NotaRepository(AudiSoftSchoolDbContext context)
        : base(context)
    {
    }

    public async Task<List<Nota>> GetByProfesorAsync(int idProfesor)
    {
        return await _dbSet
            .Where(n => n.IdProfesor == idProfesor && !n.IsDeleted)
            .Include(n => n.Profesor)
            .Include(n => n.Estudiante)
            .ToListAsync();
    }

    public async Task<List<Nota>> GetByEstudianteAsync(int idEstudiante)
    {
        return await _dbSet
            .Where(n => n.IdEstudiante == idEstudiante && !n.IsDeleted)
            .Include(n => n.Profesor)
            .Include(n => n.Estudiante)
            .ToListAsync();
    }

    public async Task<Nota?> GetByProfesorAndEstudianteAsync(int idProfesor, int idEstudiante)
    {
        return await _dbSet
            .FirstOrDefaultAsync(n => n.IdProfesor == idProfesor && 
                                       n.IdEstudiante == idEstudiante && 
                                       !n.IsDeleted);
    }
}

using AudiSoft.School.Application.Interfaces;
using AudiSoft.School.Domain.Entities;
using AudiSoft.School.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AudiSoft.School.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio para Estudiantes.
/// </summary>
public class EstudianteRepository : Repository<Estudiante>, IEstudianteRepository
{
    public EstudianteRepository(AudiSoftSchoolDbContext context)
        : base(context)
    {
    }

    public async Task<Estudiante?> GetByNombreAsync(string nombre)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Nombre == nombre && !e.IsDeleted);
    }
}

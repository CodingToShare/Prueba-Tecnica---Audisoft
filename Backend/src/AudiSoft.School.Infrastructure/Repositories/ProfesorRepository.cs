using AudiSoft.School.Application.Interfaces;
using AudiSoft.School.Domain.Entities;
using AudiSoft.School.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AudiSoft.School.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio para Profesores.
/// </summary>
public class ProfesorRepository : Repository<Profesor>, IProfesorRepository
{
    public ProfesorRepository(AudiSoftSchoolDbContext context)
        : base(context)
    {
    }

    public async Task<Profesor?> GetByNombreAsync(string nombre)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(p => p.Nombre == nombre && !p.IsDeleted);
    }
}

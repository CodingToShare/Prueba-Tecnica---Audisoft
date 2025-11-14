using System.Linq.Expressions;
using AudiSoft.School.Application.Interfaces;
using AudiSoft.School.Domain.Entities;
using AudiSoft.School.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AudiSoft.School.Infrastructure.Repositories;

/// <summary>
/// Implementación genérica del repositorio.
/// Proporciona operaciones CRUD con manejo de transacciones.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AudiSoftSchoolDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AudiSoftSchoolDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// Expone un IQueryable para consultas avanzadas desde la capa de aplicación.
    /// Por defecto se devuelve el DbSet; el llamador puede aplicar AsNoTracking().
    /// </summary>
    public IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }

    /// <summary>
    /// Obtiene una entidad por su ID.
    /// </summary>
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Obtiene todas las entidades.
    /// </summary>
    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Encuentra entidades que cumplan con el predicado.
    /// Ejecuta la consulta en la base de datos para mejor rendimiento.
    /// </summary>
    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Agrega una nueva entidad con soporte de transacción.
    /// </summary>
    public async Task<T> AddAsync(T entity)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return entity;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Actualiza una entidad existente con soporte de transacción.
    /// </summary>
    public async Task<T> UpdateAsync(T entity)
    {
        // Verificar si estamos usando InMemory database
        if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            // InMemory no soporta transacciones, usar directamente
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return entity;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Elimina (marca como eliminado) una entidad por su ID con soporte de transacción.
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Verifica si una entidad existe por su ID.
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.FindAsync(id) != null;
    }
}

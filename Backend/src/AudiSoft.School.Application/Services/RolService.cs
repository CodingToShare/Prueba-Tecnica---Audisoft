using AudiSoft.School.Application.Common;
using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Application.Extensions;
using AudiSoft.School.Application.Interfaces;
using AudiSoft.School.Domain.Entities;
using AudiSoft.School.Domain.Exceptions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AudiSoft.School.Application.Services;

/// <summary>
/// Servicio de aplicación para gestión de Roles.
/// </summary>
public class RolService
{
    private readonly IRolRepository _rolRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<RolService> _logger;

    public RolService(
        IRolRepository rolRepository,
        IMapper mapper,
        ILogger<RolService> logger)
    {
        _rolRepository = rolRepository ?? throw new ArgumentNullException(nameof(rolRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene un rol por su ID.
    /// </summary>
    public async Task<RolDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Obteniendo rol con ID: {RolId}", id);
        
        var rol = await _rolRepository.GetByIdAsync(id);
        return rol == null ? null : _mapper.Map<RolDto>(rol);
    }

    /// <summary>
    /// Obtiene un rol por su nombre.
    /// </summary>
    public async Task<RolDto?> GetByNombreAsync(string nombre)
    {
        _logger.LogInformation("Obteniendo rol con nombre: {RolNombre}", nombre);
        
        var rol = await _rolRepository.GetByNombreAsync(nombre);
        return rol == null ? null : _mapper.Map<RolDto>(rol);
    }

    /// <summary>
    /// Obtiene todos los roles.
    /// </summary>
    public async Task<List<RolDto>> GetAllAsync()
    {
        _logger.LogInformation("Obteniendo todos los roles");
        
        var roles = await _rolRepository.GetAllAsync();
        return _mapper.Map<List<RolDto>>(roles);
    }

    /// <summary>
    /// Obtiene roles activos.
    /// </summary>
    public async Task<List<RolDto>> GetActiveRolesAsync()
    {
        _logger.LogInformation("Obteniendo roles activos");
        
        var roles = await _rolRepository.GetActiveRolesAsync();
        return _mapper.Map<List<RolDto>>(roles);
    }

    /// <summary>
    /// Obtiene roles con paginación y filtros.
    /// </summary>
    public async Task<PagedResult<RolDto>> GetPagedAsync(QueryParams queryParams)
    {
        _logger.LogInformation("Obteniendo roles paginados con parámetros: {@QueryParams}", queryParams);

        IQueryable<Rol> query = _rolRepository.Query()
            .AsNoTracking()
            .Include(r => r.UsuarioRoles.Where(ur => !ur.IsDeleted));

        query = query.ApplyFilter(queryParams.Filter, queryParams.FilterField, queryParams.FilterValue);
        query = query.ApplySorting(queryParams.SortField, queryParams.SortDesc);

        return await query.ApplyPagingAsync<Rol, RolDto>(queryParams, r => _mapper.Map<RolDto>(r));
    }

    /// <summary>
    /// Crea un nuevo rol.
    /// </summary>
    public async Task<RolDto> CreateAsync(CreateRolDto dto)
    {
        _logger.LogInformation("Iniciando creación de rol: {RolNombre}", dto.Nombre);

        // Validar unicidad del nombre
        if (await _rolRepository.ExistsByNombreAsync(dto.Nombre))
        {
            throw new DuplicateEntityException("Rol", "Nombre", dto.Nombre);
        }

        try
        {
            var rol = _mapper.Map<Rol>(dto);
            var createdRol = await _rolRepository.AddAsync(rol);

            _logger.LogInformation("Rol creado exitosamente con ID: {RolId} y nombre: {RolNombre}", 
                createdRol.Id, createdRol.Nombre);

            return _mapper.Map<RolDto>(createdRol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear rol: {RolNombre}", dto.Nombre);
            throw;
        }
    }

    /// <summary>
    /// Actualiza un rol existente.
    /// </summary>
    public async Task<RolDto> UpdateAsync(int id, UpdateRolDto dto)
    {
        _logger.LogInformation("Actualizando rol con ID: {RolId}", id);

        var rol = await _rolRepository.GetByIdAsync(id);
        if (rol == null)
        {
            throw new EntityNotFoundException(nameof(Rol), id);
        }

        // Validar unicidad del nombre si cambió
        if (dto.Nombre != rol.Nombre && await _rolRepository.ExistsByNombreAsync(dto.Nombre, id))
        {
            throw new DuplicateEntityException("Rol", "Nombre", dto.Nombre);
        }

        try
        {
            rol.Nombre = dto.Nombre;
            rol.Descripcion = dto.Descripcion;
            rol.IsActive = dto.IsActive;
            rol.UpdatedAt = DateTime.UtcNow;

            var updatedRol = await _rolRepository.UpdateAsync(rol);

            _logger.LogInformation("Rol actualizado exitosamente: {RolId}", id);

            return _mapper.Map<RolDto>(updatedRol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar rol: {RolId}", id);
            throw;
        }
    }

    /// <summary>
    /// Elimina un rol (soft delete).
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Eliminando rol: {RolId}", id);

        var rol = await _rolRepository.GetByIdAsync(id);
        if (rol == null)
        {
            throw new EntityNotFoundException(nameof(Rol), id);
        }

        // Verificar si hay usuarios asignados
        var rolesWithUserCount = await _rolRepository.GetRolesWithUserCountAsync();
        var rolWithCount = rolesWithUserCount.FirstOrDefault(r => r.Rol.Id == id);
        
        if (rolWithCount.UsuarioCount > 0)
        {
            throw new BusinessRuleViolationException(
                "DeleteRolWithUsers", 
                nameof(Rol), 
                id, 
                $"No se puede eliminar el rol '{rol.Nombre}' porque tiene {rolWithCount.UsuarioCount} usuarios asignados");
        }

        try
        {
            rol.IsDeleted = true;
            rol.DeletedAt = DateTime.UtcNow;
            await _rolRepository.UpdateAsync(rol);

            _logger.LogInformation("Rol eliminado exitosamente: {RolId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar rol: {RolId}", id);
            throw;
        }
    }

    /// <summary>
    /// Obtiene todos los usuarios que tienen asignado un rol específico
    /// </summary>
    /// <param name="rolId">ID del rol</param>
    /// <param name="incluirInactivos">Si incluir usuarios inactivos</param>
    /// <returns>Lista de usuarios con el rol</returns>
    public async Task<IEnumerable<UsuarioDto>> GetUsuariosByRolIdAsync(int rolId, bool incluirInactivos = false)
    {
        _logger.LogInformation("Obteniendo usuarios con rol {RolId}", rolId);

        try
        {
            var rol = await _rolRepository.Query()
                .Include(r => r.UsuarioRoles.Where(ur => !ur.IsDeleted))
                    .ThenInclude(ur => ur.Usuario)
                        .ThenInclude(u => u.Profesor)
                .Include(r => r.UsuarioRoles.Where(ur => !ur.IsDeleted))
                    .ThenInclude(ur => ur.Usuario)
                        .ThenInclude(u => u.Estudiante)
                .FirstOrDefaultAsync(r => r.Id == rolId);

            if (rol == null)
            {
                throw new EntityNotFoundException("Rol", rolId);
            }

            var usuarios = rol.UsuarioRoles
                .Where(ur => incluirInactivos || ur.Usuario.IsActive)
                .Where(ur => ur.ValidoHasta == null || ur.ValidoHasta > DateTime.UtcNow)
                .Select(ur => _mapper.Map<UsuarioDto>(ur.Usuario))
                .ToList();

            _logger.LogInformation("Encontrados {Count} usuarios con rol {RolId}", usuarios.Count, rolId);

            return usuarios;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios con rol {RolId}", rolId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene todos los roles activos (sin paginación)
    /// </summary>
    /// <returns>Lista de roles activos</returns>
    public async Task<IEnumerable<RolDto>> GetActivosAsync()
    {
        _logger.LogInformation("Obteniendo roles activos");

        try
        {
            var roles = await _rolRepository.Query()
                .Where(r => r.IsActive)
                .AsNoTracking()
                .ToListAsync();

            var rolesDto = roles.Select(r => _mapper.Map<RolDto>(r)).ToList();

            _logger.LogInformation("Encontrados {Count} roles activos", rolesDto.Count);

            return rolesDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener roles activos");
            throw;
        }
    }

    /// <summary>
    /// Cambia el estado activo/inactivo de un rol
    /// </summary>
    /// <param name="rolId">ID del rol</param>
    /// <param name="isActive">Nuevo estado activo</param>
    public async Task CambiarEstadoAsync(int rolId, bool isActive)
    {
        _logger.LogInformation("Cambiando estado de rol {RolId} a {IsActive}", rolId, isActive);

        try
        {
            var rol = await _rolRepository.GetByIdAsync(rolId);
            if (rol == null)
            {
                throw new EntityNotFoundException("Rol", rolId);
            }

            // Si se intenta desactivar, verificar que no tenga usuarios activos
            if (!isActive)
            {
                var usuariosActivos = await _rolRepository.Query()
                    .Where(r => r.Id == rolId)
                    .SelectMany(r => r.UsuarioRoles)
                    .Where(ur => !ur.IsDeleted && ur.Usuario.IsActive)
                    .Where(ur => ur.ValidoHasta == null || ur.ValidoHasta > DateTime.UtcNow)
                    .CountAsync();

                if (usuariosActivos > 0)
                {
                    throw new InvalidEntityStateException(
                        $"No se puede desactivar el rol '{rol.Nombre}' porque tiene {usuariosActivos} usuario(s) activo(s) asignado(s)");
                }
            }

            rol.IsActive = isActive;
            rol.UpdatedAt = DateTime.UtcNow;
            rol.UpdatedBy = "System"; // En una implementación real, esto vendría del contexto de usuario

            await _rolRepository.UpdateAsync(rol);

            _logger.LogInformation("Estado del rol {RolId} cambiado a {IsActive} exitosamente", rolId, isActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado del rol {RolId}", rolId);
            throw;
        }
    }
}
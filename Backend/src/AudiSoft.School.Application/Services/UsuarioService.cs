using AudiSoft.School.Application.Common;
using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Application.Extensions;
using AudiSoft.School.Application.Interfaces;
using AudiSoft.School.Domain.Entities;
using AudiSoft.School.Domain.Exceptions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace AudiSoft.School.Application.Services;

/// <summary>
/// Servicio de aplicación para gestión de Usuarios.
/// </summary>
public class UsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IUsuarioRolRepository _usuarioRolRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        IUsuarioRolRepository usuarioRolRepository,
        IMapper mapper,
        ILogger<UsuarioService> logger)
    {
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _rolRepository = rolRepository ?? throw new ArgumentNullException(nameof(rolRepository));
        _usuarioRolRepository = usuarioRolRepository ?? throw new ArgumentNullException(nameof(usuarioRolRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene un usuario por su ID.
    /// </summary>
    public async Task<UsuarioDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Obteniendo usuario con ID: {UsuarioId}", id);
        
        var usuario = await _usuarioRepository.GetWithRolesAsync(id);
        return usuario == null ? null : _mapper.Map<UsuarioDto>(usuario);
    }

    /// <summary>
    /// Obtiene un usuario por nombre de usuario.
    /// </summary>
    public async Task<UsuarioDto?> GetByUserNameAsync(string userName)
    {
        _logger.LogInformation("Obteniendo usuario con UserName: {UserName}", userName);
        
        var usuario = await _usuarioRepository.GetByUserNameWithRolesAsync(userName);
        return usuario == null ? null : _mapper.Map<UsuarioDto>(usuario);
    }

    /// <summary>
    /// Obtiene usuarios con paginación y filtros.
    /// </summary>
    public async Task<PagedResult<UsuarioDto>> GetPagedAsync(QueryParams queryParams)
    {
        _logger.LogInformation("Obteniendo usuarios paginados con parámetros: {@QueryParams}", queryParams);

        IQueryable<Usuario> query = _usuarioRepository.Query()
            .AsNoTracking()
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .Include(u => u.Profesor)
            .Include(u => u.Estudiante);

        query = query.ApplyFilter(queryParams.Filter, queryParams.FilterField, queryParams.FilterValue);
        query = query.ApplySorting(queryParams.SortField, queryParams.SortDesc);

        return await query.ApplyPagingAsync<Usuario, UsuarioDto>(queryParams, u => _mapper.Map<UsuarioDto>(u));
    }

    /// <summary>
    /// Crea un nuevo usuario.
    /// </summary>
    public async Task<UsuarioDto> CreateAsync(CreateUsuarioDto dto)
    {
        _logger.LogInformation("Iniciando creación de usuario: {UserName}", dto.UserName);

        // Validar unicidad del nombre de usuario
        if (await _usuarioRepository.ExistsByUserNameAsync(dto.UserName))
        {
            throw new DuplicateEntityException("Usuario", "UserName", dto.UserName);
        }

        // Validar email si se proporciona
        if (!string.IsNullOrEmpty(dto.Email))
        {
            var existingUserByEmail = await _usuarioRepository.GetByEmailAsync(dto.Email);
            if (existingUserByEmail != null)
            {
                throw new DuplicateEntityException("Usuario", "Email", dto.Email);
            }
        }

        try
        {
            // Crear usuario
            var usuario = _mapper.Map<Usuario>(dto);
            usuario.PasswordHash = HashPassword(dto.Password);

            var createdUsuario = await _usuarioRepository.AddAsync(usuario);

            // Asignar roles
            if (dto.RoleIds.Any())
            {
                foreach (var roleId in dto.RoleIds)
                {
                    var rol = await _rolRepository.GetByIdAsync(roleId);
                    if (rol != null)
                    {
                        await _usuarioRolRepository.AsignarRolAsync(createdUsuario.Id, roleId, "System");
                    }
                }
            }

            _logger.LogInformation("Usuario creado exitosamente con ID: {UsuarioId} y UserName: {UserName}", 
                createdUsuario.Id, createdUsuario.UserName);

            // Retornar usuario completo con roles
            return await GetByIdAsync(createdUsuario.Id) ?? throw new InvalidOperationException("Error al recuperar usuario creado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario: {UserName}", dto.UserName);
            throw;
        }
    }

    /// <summary>
    /// Actualiza un usuario existente.
    /// </summary>
    public async Task<UsuarioDto> UpdateAsync(int id, UpdateUsuarioDto dto)
    {
        _logger.LogInformation("Actualizando usuario con ID: {UsuarioId}", id);

        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
        {
            throw new EntityNotFoundException(nameof(Usuario), id);
        }

        // Validar email si se proporciona y cambió
        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != usuario.Email)
        {
            var existingUserByEmail = await _usuarioRepository.GetByEmailAsync(dto.Email);
            if (existingUserByEmail != null && existingUserByEmail.Id != id)
            {
                throw new DuplicateEntityException("Usuario", "Email", dto.Email);
            }
        }

        try
        {
            // Actualizar propiedades
            usuario.Email = dto.Email;
            usuario.IdProfesor = dto.IdProfesor;
            usuario.IdEstudiante = dto.IdEstudiante;
            usuario.IsActive = dto.IsActive;
            usuario.UpdatedAt = DateTime.UtcNow;

            await _usuarioRepository.UpdateAsync(usuario);

            _logger.LogInformation("Usuario actualizado exitosamente: {UsuarioId}", id);

            return await GetByIdAsync(id) ?? throw new InvalidOperationException("Error al recuperar usuario actualizado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario: {UsuarioId}", id);
            throw;
        }
    }

    /// <summary>
    /// Cambia la contraseña de un usuario.
    /// </summary>
    public async Task<bool> ChangePasswordAsync(int id, ChangePasswordDto dto)
    {
        _logger.LogInformation("Cambiando contraseña para usuario: {UsuarioId}", id);

        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
        {
            throw new EntityNotFoundException(nameof(Usuario), id);
        }

        // Verificar contraseña actual
        if (!VerifyPassword(dto.CurrentPassword, usuario.PasswordHash))
        {
            _logger.LogWarning("Intento de cambio de contraseña con contraseña actual incorrecta para usuario: {UsuarioId}", id);
            throw new InvalidEntityStateException("La contraseña actual no es correcta");
        }

        try
        {
            usuario.PasswordHash = HashPassword(dto.NewPassword);
            usuario.UpdatedAt = DateTime.UtcNow;

            await _usuarioRepository.UpdateAsync(usuario);

            _logger.LogInformation("Contraseña cambiada exitosamente para usuario: {UsuarioId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar contraseña para usuario: {UsuarioId}", id);
            throw;
        }
    }

    /// <summary>
    /// Asigna un rol a un usuario.
    /// </summary>
    public async Task<bool> AsignarRolAsync(int usuarioId, int rolId)
    {
        _logger.LogInformation("Asignando rol {RolId} a usuario {UsuarioId}", rolId, usuarioId);

        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario == null)
        {
            throw new EntityNotFoundException(nameof(Usuario), usuarioId);
        }

        var rol = await _rolRepository.GetByIdAsync(rolId);
        if (rol == null)
        {
            throw new EntityNotFoundException(nameof(Rol), rolId);
        }

        try
        {
            await _usuarioRolRepository.AsignarRolAsync(usuarioId, rolId, "System");
            _logger.LogInformation("Rol {RolNombre} asignado exitosamente a usuario {UserName}", rol.Nombre, usuario.UserName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar rol {RolId} a usuario {UsuarioId}", rolId, usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Remueve un rol de un usuario.
    /// </summary>
    public async Task<bool> RemoverRolAsync(int usuarioId, int rolId)
    {
        _logger.LogInformation("Removiendo rol {RolId} de usuario {UsuarioId}", rolId, usuarioId);

        try
        {
            var removed = await _usuarioRolRepository.RemoverRolAsync(usuarioId, rolId);
            if (removed)
            {
                _logger.LogInformation("Rol removido exitosamente de usuario {UsuarioId}", usuarioId);
            }
            else
            {
                _logger.LogWarning("No se encontró asignación de rol {RolId} para usuario {UsuarioId}", rolId, usuarioId);
            }
            return removed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al remover rol {RolId} de usuario {UsuarioId}", rolId, usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Elimina un usuario (soft delete).
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Eliminando usuario: {UsuarioId}", id);

        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
        {
            throw new EntityNotFoundException(nameof(Usuario), id);
        }

        try
        {
            usuario.IsDeleted = true;
            usuario.DeletedAt = DateTime.UtcNow;
            await _usuarioRepository.UpdateAsync(usuario);

            _logger.LogInformation("Usuario eliminado exitosamente: {UsuarioId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario: {UsuarioId}", id);
            throw;
        }
    }

    /// <summary>
    /// Genera hash de contraseña usando SHA256.
    /// </summary>
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "AudiSoft_Salt_2024"));
        return Convert.ToBase64String(hashedBytes);
    }

    /// <summary>
    /// Verifica una contraseña contra su hash.
    /// </summary>
    private static bool VerifyPassword(string password, string hash)
    {
        var passwordHash = HashPassword(password);
        return passwordHash == hash;
    }

    /// <summary>
    /// Obtiene los roles activos de un usuario por su ID
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <returns>Lista de roles activos del usuario</returns>
    public async Task<IEnumerable<RolDto>> GetRolesByUsuarioIdAsync(int usuarioId)
    {
        _logger.LogInformation("Obteniendo roles del usuario {UsuarioId}", usuarioId);

        try
        {
            var usuario = await _usuarioRepository.Query()
                .Include(u => u.UsuarioRoles.Where(ur => !ur.IsDeleted && 
                    (ur.ValidoHasta == null || ur.ValidoHasta > DateTime.UtcNow)))
                    .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                throw new EntityNotFoundException("Usuario", usuarioId);
            }

            var roles = usuario.UsuarioRoles
                .Where(ur => ur.Rol.IsActive)
                .Select(ur => _mapper.Map<RolDto>(ur.Rol))
                .ToList();

            _logger.LogInformation("Encontrados {Count} roles para usuario {UsuarioId}", roles.Count, usuarioId);

            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener roles del usuario {UsuarioId}", usuarioId);
            throw;
        }
    }

    /// <summary>
    /// Cambia la contraseña de un usuario
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="nuevaPassword">Nueva contraseña</param>
    public async Task CambiarPasswordAsync(int usuarioId, string nuevaPassword)
    {
        _logger.LogInformation("Cambiando contraseña para usuario {UsuarioId}", usuarioId);

        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
            {
                throw new EntityNotFoundException("Usuario", usuarioId);
            }

            usuario.PasswordHash = HashPassword(nuevaPassword);
            usuario.UpdatedAt = DateTime.UtcNow;
            usuario.UpdatedBy = "System"; // En una implementación real, esto vendría del contexto de usuario

            await _usuarioRepository.UpdateAsync(usuario);

            _logger.LogInformation("Contraseña cambiada exitosamente para usuario {UsuarioId}", usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar contraseña del usuario {UsuarioId}", usuarioId);
            throw;
        }
    }
}
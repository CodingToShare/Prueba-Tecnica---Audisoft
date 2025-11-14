using AudiSoft.School.Application.Common;
using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AudiSoft.School.Api.Controllers;

/// <summary>
/// Controlador para gestión de roles
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly RolService _rolService;
    private readonly ILogger<RolesController> _logger;

    /// <summary>
    /// Constructor del controlador de roles
    /// </summary>
    /// <param name="rolService">Servicio de roles</param>
    /// <param name="logger">Logger para el controlador</param>
    public RolesController(RolService rolService, ILogger<RolesController> logger)
    {
        _rolService = rolService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los roles con paginación y filtrado
    /// </summary>
    /// <param name="queryParams">Parámetros de consulta</param>
    /// <returns>Lista paginada de roles</returns>
    /// <response code="200">Lista de roles obtenida correctamente</response>
    /// <response code="400">Parámetros de consulta inválidos</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<RolDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRoles([FromQuery] QueryParams queryParams)
    {
        _logger.LogInformation("GET Roles solicitado con parámetros: {@QueryParams}", queryParams);

        var result = await _rolService.GetPagedAsync(queryParams);

        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();

        return Ok(result);
    }

    /// <summary>
    /// Obtiene un rol por ID
    /// </summary>
    /// <param name="id">ID del rol</param>
    /// <returns>Rol encontrado</returns>
    /// <response code="200">Rol encontrado</response>
    /// <response code="404">Rol no encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RolDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRol(int id)
    {
        _logger.LogInformation("GET Rol {Id} solicitado", id);

        var rol = await _rolService.GetByIdAsync(id);

        return Ok(rol);
    }

    /// <summary>
    /// Crea un nuevo rol
    /// </summary>
    /// <param name="dto">Datos del rol a crear</param>
    /// <returns>Rol creado</returns>
    /// <response code="201">Rol creado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="409">Rol duplicado (mismo nombre)</response>
    [HttpPost]
    [ProducesResponseType(typeof(RolDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRol([FromBody] CreateRolDto dto)
    {
        _logger.LogInformation("POST Rol solicitado para: {Nombre}", dto.Nombre);

        var rol = await _rolService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetRol),
            new { id = rol.Id },
            rol);
    }

    /// <summary>
    /// Actualiza un rol existente
    /// </summary>
    /// <param name="id">ID del rol a actualizar</param>
    /// <param name="dto">Nuevos datos del rol</param>
    /// <returns>Rol actualizado</returns>
    /// <response code="200">Rol actualizado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Rol no encontrado</response>
    /// <response code="409">Nombre duplicado</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RolDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateRol(int id, [FromBody] UpdateRolDto dto)
    {
        _logger.LogInformation("PUT Rol {Id} solicitado", id);

        var rol = await _rolService.UpdateAsync(id, dto);

        return Ok(rol);
    }

    /// <summary>
    /// Elimina un rol (soft delete)
    /// </summary>
    /// <param name="id">ID del rol a eliminar</param>
    /// <returns>Resultado de la eliminación</returns>
    /// <response code="204">Rol eliminado exitosamente</response>
    /// <response code="404">Rol no encontrado</response>
    /// <response code="400">No se puede eliminar un rol que tiene usuarios asignados</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRol(int id)
    {
        _logger.LogInformation("DELETE Rol {Id} solicitado", id);

        await _rolService.DeleteAsync(id);

        return NoContent();
    }

    /// <summary>
    /// Obtiene todos los usuarios que tienen asignado un rol específico
    /// </summary>
    /// <param name="id">ID del rol</param>
    /// <param name="includeInactive">Incluir usuarios inactivos</param>
    /// <returns>Lista de usuarios con el rol</returns>
    /// <response code="200">Lista de usuarios obtenida correctamente</response>
    /// <response code="404">Rol no encontrado</response>
    [HttpGet("{id}/usuarios")]
    [ProducesResponseType(typeof(IEnumerable<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUsuariosByRol(int id, [FromQuery] bool includeInactive = false)
    {
        _logger.LogInformation("GET Usuarios con rol {Id} solicitado", id);

        var usuarios = await _rolService.GetUsuariosByRolIdAsync(id, includeInactive);

        return Ok(usuarios);
    }

    /// <summary>
    /// Obtiene todos los roles activos (sin paginación, para dropdowns)
    /// </summary>
    /// <returns>Lista de roles activos</returns>
    /// <response code="200">Lista de roles obtenida correctamente</response>
    [HttpGet("activos")]
    [ProducesResponseType(typeof(IEnumerable<RolDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolesActivos()
    {
        _logger.LogInformation("GET Roles activos solicitado");

        var roles = await _rolService.GetActivosAsync();

        return Ok(roles);
    }

    /// <summary>
    /// Activa o desactiva un rol
    /// </summary>
    /// <param name="id">ID del rol</param>
    /// <param name="request">Estado activo a establecer</param>
    /// <returns>Resultado del cambio de estado</returns>
    /// <response code="200">Estado cambiado exitosamente</response>
    /// <response code="404">Rol no encontrado</response>
    /// <response code="400">No se puede desactivar un rol con usuarios activos</response>
    [HttpPost("{id}/cambiar-estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoRolDto request)
    {
        _logger.LogInformation("POST Cambiar estado de rol {Id} a {IsActive}", id, request.IsActive);

        await _rolService.CambiarEstadoAsync(id, request.IsActive);

        return Ok(new { Message = $"Rol {(request.IsActive ? "activado" : "desactivado")} correctamente" });
    }
}

/// <summary>
/// DTO para cambio de estado de rol
/// </summary>
public class CambiarEstadoRolDto
{
    /// <summary>
    /// Nuevo estado activo
    /// </summary>
    public bool IsActive { get; set; }
}
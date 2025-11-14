using AudiSoft.School.Application.Common;
using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AudiSoft.School.Api.Controllers;

/// <summary>
/// Controlador para gestión de usuarios - Solo administradores
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize(Policy = "AdminOnly")]
[SwaggerTag("Administración de usuarios del sistema - Solo administradores")]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioService _usuarioService;
    private readonly ILogger<UsuariosController> _logger;

    /// <summary>
    /// Constructor del controlador de usuarios
    /// </summary>
    /// <param name="usuarioService">Servicio de usuarios</param>
    /// <param name="logger">Logger para el controlador</param>
    public UsuariosController(UsuarioService usuarioService, ILogger<UsuariosController> logger)
    {
        _usuarioService = usuarioService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los usuarios con paginación y filtrado
    /// </summary>
    /// <param name="queryParams">Parámetros de consulta</param>
    /// <returns>Lista paginada de usuarios</returns>
    /// <response code="200">Lista de usuarios obtenida correctamente</response>
    /// <response code="400">Parámetros de consulta inválidos</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUsuarios([FromQuery] QueryParams queryParams)
    {
        _logger.LogInformation("GET Usuarios solicitado con parámetros: {@QueryParams}", queryParams);

        var result = await _usuarioService.GetPagedAsync(queryParams);

        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();

        return Ok(result);
    }

    /// <summary>
    /// Obtiene un usuario por ID
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Usuario encontrado</returns>
    /// <response code="200">Usuario encontrado</response>
    /// <response code="404">Usuario no encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUsuario(int id)
    {
        _logger.LogInformation("GET Usuario {Id} solicitado", id);

        var usuario = await _usuarioService.GetByIdAsync(id);

        return Ok(usuario);
    }

    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    /// <param name="dto">Datos del usuario a crear</param>
    /// <returns>Usuario creado</returns>
    /// <response code="201">Usuario creado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="409">Usuario duplicado (mismo username o email)</response>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUsuario([FromBody] CreateUsuarioDto dto)
    {
        _logger.LogInformation("POST Usuario solicitado para: {UserName}", dto.UserName);

        var usuario = await _usuarioService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetUsuario),
            new { id = usuario.Id },
            usuario);
    }

    /// <summary>
    /// Actualiza un usuario existente
    /// </summary>
    /// <param name="id">ID del usuario a actualizar</param>
    /// <param name="dto">Nuevos datos del usuario</param>
    /// <returns>Usuario actualizado</returns>
    /// <response code="200">Usuario actualizado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Usuario no encontrado</response>
    /// <response code="409">Email duplicado</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateUsuario(int id, [FromBody] UpdateUsuarioDto dto)
    {
        _logger.LogInformation("PUT Usuario {Id} solicitado", id);

        var usuario = await _usuarioService.UpdateAsync(id, dto);

        return Ok(usuario);
    }

    /// <summary>
    /// Elimina un usuario (soft delete)
    /// </summary>
    /// <param name="id">ID del usuario a eliminar</param>
    /// <returns>Resultado de la eliminación</returns>
    /// <response code="204">Usuario eliminado exitosamente</response>
    /// <response code="404">Usuario no encontrado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUsuario(int id)
    {
        _logger.LogInformation("DELETE Usuario {Id} solicitado", id);

        await _usuarioService.DeleteAsync(id);

        return NoContent();
    }

    /// <summary>
    /// Asigna un rol a un usuario
    /// </summary>
    /// <param name="idUsuario">ID del usuario</param>
    /// <param name="idRol">ID del rol</param>
    /// <returns>Resultado de la asignación</returns>
    /// <response code="200">Rol asignado exitosamente</response>
    /// <response code="404">Usuario o rol no encontrado</response>
    /// <response code="409">El usuario ya tiene ese rol</response>
    [HttpPost("{idUsuario}/roles/{idRol}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AsignarRol(int idUsuario, int idRol)
    {
        _logger.LogInformation("POST Asignar rol {IdRol} a usuario {IdUsuario}", idRol, idUsuario);

        await _usuarioService.AsignarRolAsync(idUsuario, idRol);

        return Ok(new { Message = "Rol asignado correctamente" });
    }

    /// <summary>
    /// Remueve un rol de un usuario
    /// </summary>
    /// <param name="idUsuario">ID del usuario</param>
    /// <param name="idRol">ID del rol</param>
    /// <returns>Resultado de la remoción</returns>
    /// <response code="200">Rol removido exitosamente</response>
    /// <response code="404">Usuario, rol o asignación no encontrada</response>
    [HttpDelete("{idUsuario}/roles/{idRol}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverRol(int idUsuario, int idRol)
    {
        _logger.LogInformation("DELETE Remover rol {IdRol} de usuario {IdUsuario}", idRol, idUsuario);

        await _usuarioService.RemoverRolAsync(idUsuario, idRol);

        return Ok(new { Message = "Rol removido correctamente" });
    }

    /// <summary>
    /// Obtiene los roles activos de un usuario
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Lista de roles del usuario</returns>
    /// <response code="200">Roles obtenidos correctamente</response>
    /// <response code="404">Usuario no encontrado</response>
    [HttpGet("{id}/roles")]
    [ProducesResponseType(typeof(IEnumerable<RolDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRolesUsuario(int id)
    {
        _logger.LogInformation("GET Roles de usuario {Id} solicitado", id);

        var roles = await _usuarioService.GetRolesByUsuarioIdAsync(id);

        return Ok(roles);
    }

    /// <summary>
    /// Cambia la contraseña de un usuario
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="request">Datos de cambio de contraseña</param>
    /// <returns>Resultado del cambio</returns>
    /// <response code="200">Contraseña cambiada exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="404">Usuario no encontrado</response>
    [HttpPost("{id}/cambiar-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarPassword(int id, [FromBody] CambiarPasswordDto request)
    {
        _logger.LogInformation("POST Cambiar password para usuario {Id}", id);

        await _usuarioService.CambiarPasswordAsync(id, request.NuevaPassword);

        return Ok(new { Message = "Contraseña actualizada correctamente" });
    }
}

/// <summary>
/// DTO para cambio de contraseña
/// </summary>
public class CambiarPasswordDto
{
    /// <summary>
    /// Nueva contraseña
    /// </summary>
    public string NuevaPassword { get; set; } = string.Empty;
}
using AudiSoft.School.Application.Common;
using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Application.Extensions;
using AudiSoft.School.Application.Services;
using AudiSoft.School.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AudiSoft.School.Api.Controllers;

/// <summary>
/// API endpoints para gestión de Profesores.
/// Solo Admin puede gestionar profesores completamente.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[SwaggerTag("Gestión completa de profesores - Solo administradores (CRUD, búsqueda, paginación)")]
public class ProfesoresController : ControllerBase
{
    private readonly ProfesorService _service;
    private readonly ILogger<ProfesoresController> _logger;

    public ProfesoresController(ProfesorService service, ILogger<ProfesoresController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene todos los profesores.
    /// Admin puede ver la lista completa, Profesores pueden ver la lista para dropdowns.
    /// </summary>
    /// <returns>Lista de profesores</returns>
    /// <response code="200">Operación exitosa</response>
    /// <response code="403">Sin permisos para ver profesores</response>
        [HttpGet]
        [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProfesorDto>>> GetAll([FromQuery] AudiSoft.School.Application.Common.QueryParams queryParams)
    {
        _logger.LogInformation("Obteniendo todos los profesores");
        var result = await _service.GetPagedAsync(queryParams);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(result);
    }

    /// <summary>
    /// Búsqueda avanzada de profesores mediante expresión en QueryParams.Filter
    /// </summary>
        [HttpGet("search")]
        [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProfesorDto>>> Search([FromQuery] AudiSoft.School.Application.Common.QueryParams queryParams)
    {
        _logger.LogInformation("Buscando profesores con parámetros: {QueryParams}", queryParams);
        var result = await _service.GetPagedAsync(queryParams);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un profesor por su ID.
    /// </summary>
    /// <param name="id">ID del profesor</param>
    /// <returns>Datos del profesor</returns>
    /// <response code="200">Profesor encontrado</response>
    /// <response code="404">Profesor no encontrado</response>
    [HttpGet("{id}")]
        [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfesorDto>> GetById(int id)
    {
        _logger.LogInformation("Obteniendo profesor con ID {ProfesorId}", id);
        
        try
        {
            var profesor = await _service.GetByIdAsync(id);
            if (profesor == null)
            {
                _logger.LogWarning("Profesor con ID {ProfesorId} no encontrado", id);
                return NotFound(new { message = $"Profesor con ID {id} no encontrado" });
            }
            return Ok(profesor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener profesor con ID {ProfesorId}", id);
            throw;
        }
    }

    /// <summary>
    /// Crea un nuevo profesor.
    /// </summary>
    /// <param name="dto">Datos del profesor a crear</param>
    /// <returns>Profesor creado</returns>
    /// <response code="201">Profesor creado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProfesorDto>> Create([FromBody] CreateProfesorDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Intento de crear profesor con datos nulos");
            return BadRequest(new { message = "Los datos del profesor son requeridos" });
        }

        _logger.LogInformation("Creando nuevo profesor: {Nombre}", dto.Nombre);
        
        try
        {
            var profesor = await _service.CreateAsync(dto);
            _logger.LogInformation("Profesor creado exitosamente con ID {ProfesorId}", profesor.Id);
            return CreatedAtAction(nameof(GetById), new { id = profesor.Id }, profesor);
        }
        catch (InvalidEntityStateException ex)
        {
            _logger.LogWarning(ex, "Error de validación al crear profesor");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear profesor");
            throw;
        }
    }

    /// <summary>
    /// Actualiza un profesor existente.
    /// </summary>
    /// <param name="id">ID del profesor</param>
    /// <param name="dto">Datos actualizados</param>
    /// <returns>Profesor actualizado</returns>
    /// <response code="200">Profesor actualizado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="404">Profesor no encontrado</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfesorDto>> Update(int id, [FromBody] UpdateProfesorDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Intento de actualizar profesor con datos nulos");
            return BadRequest(new { message = "Los datos del profesor son requeridos" });
        }

        _logger.LogInformation("Actualizando profesor con ID {ProfesorId}", id);
        
        try
        {
            var profesor = await _service.UpdateAsync(id, dto);
            _logger.LogInformation("Profesor con ID {ProfesorId} actualizado exitosamente", id);
            return Ok(profesor);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Profesor no encontrado");
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidEntityStateException ex)
        {
            _logger.LogWarning(ex, "Error de validación al actualizar profesor");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar profesor con ID {ProfesorId}", id);
            throw;
        }
    }

    /// <summary>
    /// Elimina un profesor (soft delete).
    /// </summary>
    /// <param name="id">ID del profesor</param>
    /// <returns>Sin contenido</returns>
    /// <response code="204">Profesor eliminado exitosamente</response>
    /// <response code="404">Profesor no encontrado</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Eliminando profesor con ID {ProfesorId}", id);
        
        try
        {
            await _service.DeleteAsync(id);
            _logger.LogInformation("Profesor con ID {ProfesorId} eliminado exitosamente", id);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Profesor no encontrado");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar profesor con ID {ProfesorId}", id);
            throw;
        }
    }
}

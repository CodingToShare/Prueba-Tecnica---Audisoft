using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Application.Services;
using AudiSoft.School.Application.Common;
using AudiSoft.School.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AudiSoft.School.Api.Controllers;

/// <summary>
/// API endpoints para gestión de Notas.
/// Proporciona operaciones CRUD completas con validaciones de referencias.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class NotasController : ControllerBase
{
    private readonly NotaService _service;
    private readonly ILogger<NotasController> _logger;

    public NotasController(NotaService service, ILogger<NotasController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene todas las notas.
    /// </summary>
    /// <returns>Lista de notas</returns>
    /// <response code="200">Operación exitosa</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<NotaDto>>> GetAll([FromQuery] QueryParams queryParams)
    {
        _logger.LogInformation("Obteniendo todas las notas");
        var result = await _service.GetPagedAsync(queryParams);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene una nota por su ID.
    /// </summary>
    /// <param name="id">ID de la nota</param>
    /// <returns>Datos de la nota</returns>
    /// <response code="200">Nota encontrada</response>
    /// <response code="404">Nota no encontrada</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotaDto>> GetById(int id)
    {
        _logger.LogInformation("Obteniendo nota con ID {NotaId}", id);
        
        try
        {
            var nota = await _service.GetByIdAsync(id);
            if (nota == null)
            {
                _logger.LogWarning("Nota con ID {NotaId} no encontrada", id);
                return NotFound(new { message = $"Nota con ID {id} no encontrada" });
            }
            return Ok(nota);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener nota con ID {NotaId}", id);
            throw;
        }
    }

    /// <summary>
    /// Obtiene todas las notas de un profesor específico.
    /// </summary>
    /// <param name="idProfesor">ID del profesor</param>
    /// <returns>Lista de notas del profesor</returns>
    /// <response code="200">Operación exitosa</response>
    [HttpGet("profesor/{idProfesor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<NotaDto>>> GetByProfesor(int idProfesor, [FromQuery] QueryParams queryParams)
    {
        _logger.LogInformation("Obteniendo notas del profesor con ID {ProfesorId}", idProfesor);
        var result = await _service.GetByProfesorPagedAsync(idProfesor, queryParams);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene todas las notas de un estudiante específico.
    /// </summary>
    /// <param name="idEstudiante">ID del estudiante</param>
    /// <returns>Lista de notas del estudiante</returns>
    /// <response code="200">Operación exitosa</response>
    [HttpGet("estudiante/{idEstudiante}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<NotaDto>>> GetByEstudiante(int idEstudiante, [FromQuery] QueryParams queryParams)
    {
        _logger.LogInformation("Obteniendo notas del estudiante con ID {EstudianteId}", idEstudiante);
        var result = await _service.GetByEstudiantePagedAsync(idEstudiante, queryParams);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(result);
    }

    /// <summary>
    /// Crea una nueva nota.
    /// </summary>
    /// <param name="dto">Datos de la nota a crear</param>
    /// <returns>Nota creada</returns>
    /// <response code="201">Nota creada exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotaDto>> Create([FromBody] CreateNotaDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Intento de crear nota con datos nulos");
            return BadRequest(new { message = "Los datos de la nota son requeridos" });
        }

        _logger.LogInformation("Creando nueva nota para profesor {ProfesorId} y estudiante {EstudianteId}",
            dto.IdProfesor, dto.IdEstudiante);
        
        try
        {
            var nota = await _service.CreateAsync(dto);
            _logger.LogInformation("Nota creada exitosamente con ID {NotaId}", nota.Id);
            return CreatedAtAction(nameof(GetById), new { id = nota.Id }, nota);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Profesor o estudiante no encontrado");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidEntityStateException ex)
        {
            _logger.LogWarning(ex, "Error de validación al crear nota");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear nota");
            throw;
        }
    }

    /// <summary>
    /// Actualiza una nota existente.
    /// </summary>
    /// <param name="id">ID de la nota</param>
    /// <param name="dto">Datos actualizados</param>
    /// <returns>Nota actualizada</returns>
    /// <response code="200">Nota actualizada exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="404">Nota no encontrada</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotaDto>> Update(int id, [FromBody] UpdateNotaDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Intento de actualizar nota con datos nulos");
            return BadRequest(new { message = "Los datos de la nota son requeridos" });
        }

        _logger.LogInformation("Actualizando nota con ID {NotaId}", id);
        
        try
        {
            var nota = await _service.UpdateAsync(id, dto);
            _logger.LogInformation("Nota con ID {NotaId} actualizada exitosamente", id);
            return Ok(nota);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Nota, profesor o estudiante no encontrado");
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidEntityStateException ex)
        {
            _logger.LogWarning(ex, "Error de validación al actualizar nota");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar nota con ID {NotaId}", id);
            throw;
        }
    }

    /// <summary>
    /// Elimina una nota (soft delete).
    /// </summary>
    /// <param name="id">ID de la nota</param>
    /// <returns>Sin contenido</returns>
    /// <response code="204">Nota eliminada exitosamente</response>
    /// <response code="404">Nota no encontrada</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Eliminando nota con ID {NotaId}", id);
        
        try
        {
            await _service.DeleteAsync(id);
            _logger.LogInformation("Nota con ID {NotaId} eliminada exitosamente", id);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Nota no encontrada");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar nota con ID {NotaId}", id);
            throw;
        }
    }
}

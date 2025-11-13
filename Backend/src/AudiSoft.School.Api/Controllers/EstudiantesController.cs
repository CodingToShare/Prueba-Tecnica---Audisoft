using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Application.Services;
using AudiSoft.School.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AudiSoft.School.Api.Controllers;

/// <summary>
/// API endpoints para gestión de Estudiantes.
/// Proporciona operaciones CRUD completas.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class EstudiantesController : ControllerBase
{
    private readonly EstudianteService _service;
    private readonly ILogger<EstudiantesController> _logger;

    public EstudiantesController(EstudianteService service, ILogger<EstudiantesController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene todos los estudiantes.
    /// </summary>
    /// <returns>Lista de estudiantes</returns>
    /// <response code="200">Operación exitosa</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EstudianteDto>>> GetAll()
    {
        _logger.LogInformation("Obteniendo todos los estudiantes");
        var estudiantes = await _service.GetAllAsync();
        return Ok(estudiantes);
    }

    /// <summary>
    /// Obtiene un estudiante por su ID.
    /// </summary>
    /// <param name="id">ID del estudiante</param>
    /// <returns>Datos del estudiante</returns>
    /// <response code="200">Estudiante encontrado</response>
    /// <response code="404">Estudiante no encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstudianteDto>> GetById(int id)
    {
        _logger.LogInformation("Obteniendo estudiante con ID {EstudianteId}", id);
        
        try
        {
            var estudiante = await _service.GetByIdAsync(id);
            if (estudiante == null)
            {
                _logger.LogWarning("Estudiante con ID {EstudianteId} no encontrado", id);
                return NotFound(new { message = $"Estudiante con ID {id} no encontrado" });
            }
            return Ok(estudiante);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estudiante con ID {EstudianteId}", id);
            throw;
        }
    }

    /// <summary>
    /// Crea un nuevo estudiante.
    /// </summary>
    /// <param name="dto">Datos del estudiante a crear</param>
    /// <returns>Estudiante creado</returns>
    /// <response code="201">Estudiante creado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EstudianteDto>> Create([FromBody] CreateEstudianteDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Intento de crear estudiante con datos nulos");
            return BadRequest(new { message = "Los datos del estudiante son requeridos" });
        }

        _logger.LogInformation("Creando nuevo estudiante: {Nombre}", dto.Nombre);
        
        try
        {
            var estudiante = await _service.CreateAsync(dto);
            _logger.LogInformation("Estudiante creado exitosamente con ID {EstudianteId}", estudiante.Id);
            return CreatedAtAction(nameof(GetById), new { id = estudiante.Id }, estudiante);
        }
        catch (InvalidEntityStateException ex)
        {
            _logger.LogWarning(ex, "Error de validación al crear estudiante");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear estudiante");
            throw;
        }
    }

    /// <summary>
    /// Actualiza un estudiante existente.
    /// </summary>
    /// <param name="id">ID del estudiante</param>
    /// <param name="dto">Datos actualizados</param>
    /// <returns>Estudiante actualizado</returns>
    /// <response code="200">Estudiante actualizado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="404">Estudiante no encontrado</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstudianteDto>> Update(int id, [FromBody] CreateEstudianteDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Intento de actualizar estudiante con datos nulos");
            return BadRequest(new { message = "Los datos del estudiante son requeridos" });
        }

        _logger.LogInformation("Actualizando estudiante con ID {EstudianteId}", id);
        
        try
        {
            var estudiante = await _service.UpdateAsync(id, dto);
            _logger.LogInformation("Estudiante con ID {EstudianteId} actualizado exitosamente", id);
            return Ok(estudiante);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Estudiante no encontrado");
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidEntityStateException ex)
        {
            _logger.LogWarning(ex, "Error de validación al actualizar estudiante");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estudiante con ID {EstudianteId}", id);
            throw;
        }
    }

    /// <summary>
    /// Elimina un estudiante (soft delete).
    /// </summary>
    /// <param name="id">ID del estudiante</param>
    /// <returns>Sin contenido</returns>
    /// <response code="204">Estudiante eliminado exitosamente</response>
    /// <response code="404">Estudiante no encontrado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Eliminando estudiante con ID {EstudianteId}", id);
        
        try
        {
            await _service.DeleteAsync(id);
            _logger.LogInformation("Estudiante con ID {EstudianteId} eliminado exitosamente", id);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Estudiante no encontrado");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar estudiante con ID {EstudianteId}", id);
            throw;
        }
    }
}

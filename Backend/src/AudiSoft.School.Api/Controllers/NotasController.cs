using AudiSoft.School.Application.DTOs;
using AudiSoft.School.Application.Extensions;
using AudiSoft.School.Application.Services;
using AudiSoft.School.Application.Common;
using AudiSoft.School.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security;

namespace AudiSoft.School.Api.Controllers;

/// <summary>
/// API endpoints para gestión de Notas.
/// Admin: acceso completo, Profesor: solo sus notas, Estudiante: solo sus notas (lectura)
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[SwaggerTag("Gestión de notas y calificaciones con permisos diferenciados por rol")]
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
    /// Obtiene todas las notas según el rol del usuario.
    /// Admin: todas las notas, Profesor: solo sus notas, Estudiante: solo sus notas
    /// </summary>
    /// <returns>Lista de notas filtrada por permisos</returns>
    /// <response code="200">Operación exitosa</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<NotaDto>>> GetAll([FromQuery] QueryParams queryParams)
    {
        _logger.LogInformation("Obteniendo notas para usuario: {UserName}", User.GetUserName());
        
        // Aplicar filtros basados en el rol del usuario
        var filteredParams = ApplyUserBasedFilters(queryParams);
        
        var result = await _service.GetPagedAsync(filteredParams);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene una nota por su ID.
    /// Validaciones: Admin (todas), Profesor (sus notas), Estudiante (sus notas)
    /// </summary>
    /// <param name="id">ID de la nota</param>
    /// <returns>Datos de la nota</returns>
    /// <response code="200">Nota encontrada</response>
    /// <response code="403">Sin permisos para ver esta nota</response>
    /// <response code="404">Nota no encontrada</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotaDto>> GetById(int id)
    {
        _logger.LogInformation("Obteniendo nota con ID {NotaId} para usuario {UserName}", id, User.GetUserName());
        
        try
        {
            var nota = await _service.GetByIdAsync(id);
            if (nota == null)
            {
                _logger.LogWarning("Nota con ID {NotaId} no encontrada", id);
                return NotFound(new { message = $"Nota con ID {id} no encontrada" });
            }

            // Validar permisos de acceso
            if (!User.CanViewNota(nota.IdProfesor, nota.IdEstudiante))
            {
                _logger.LogWarning("Usuario {UserName} intentó acceder a nota {NotaId} sin permisos", User.GetUserName(), id);
                return Forbid("No tiene permisos para ver esta nota");
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
    /// <param name="queryParams">Parámetros de paginación y filtrado</param>
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
    /// <param name="queryParams">Parámetros de paginación y filtrado</param>
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
    /// <response code="401">No autorizado</response>
    /// <response code="403">Acceso prohibido</response>
    [HttpPost]
    [Authorize(Policy = "ProfesorOrAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<NotaDto>> Create([FromBody] CreateNotaDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Intento de crear nota con datos nulos");
            return BadRequest(new { message = "Los datos de la nota son requeridos" });
        }

        // Verificar si el profesor puede crear esta nota
        if (!User.CanCreateNota(dto.IdProfesor))
        {
            _logger.LogWarning("Usuario {UserId} intentó crear nota para profesor {ProfesorId} sin permisos", 
                User.GetUserId(), dto.IdProfesor);
            throw new SecurityException("No tiene permisos para crear notas para este profesor");
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
    /// <response code="401">No autorizado</response>
    /// <response code="403">Acceso prohibido</response>
    /// <response code="404">Nota no encontrada</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "ProfesorOrAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotaDto>> Update(int id, [FromBody] UpdateNotaDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Intento de actualizar nota con datos nulos");
            return BadRequest(new { message = "Los datos de la nota son requeridos" });
        }

        // Verificar si el usuario puede modificar esta nota
        if (!await CanModifyNotaAsync(id))
        {
            _logger.LogWarning("Usuario {UserId} intentó actualizar nota {NotaId} sin permisos", 
                User.GetUserId(), id);
            throw new SecurityException("No tiene permisos para modificar esta nota");
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
    /// <response code="401">No autorizado</response>
    /// <response code="403">Acceso prohibido</response>
    /// <response code="404">Nota no encontrada</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ProfesorOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        // Verificar si el usuario puede eliminar esta nota
        if (!await CanModifyNotaAsync(id))
        {
            _logger.LogWarning("Usuario {UserId} intentó eliminar nota {NotaId} sin permisos", 
                User.GetUserId(), id);
            throw new SecurityException("No tiene permisos para eliminar esta nota");
        }

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

    /// <summary>
    /// Aplica filtros basados en el rol del usuario para restringir el acceso a notas
    /// </summary>
    private QueryParams ApplyUserBasedFilters(QueryParams originalParams)
    {
        var filteredParams = new QueryParams
        {
            Page = originalParams.Page,
            PageSize = originalParams.PageSize,
            MaxPageSize = originalParams.MaxPageSize,
            SortField = originalParams.SortField,
            SortDesc = originalParams.SortDesc,
            Filter = originalParams.Filter,
            FilterField = originalParams.FilterField,
            FilterValue = originalParams.FilterValue
        };

        // Admin puede ver todas las notas
        if (User.IsAdmin())
        {
            return filteredParams;
        }

        // Profesor solo puede ver sus propias notas
        if (User.IsProfesor())
        {
            var profesorId = User.GetProfesorId();
            if (profesorId.HasValue)
            {
                // Agregar filtro por IdProfesor
                var profesorFilter = $"IdProfesor={profesorId.Value}";
                filteredParams.Filter = string.IsNullOrEmpty(filteredParams.Filter) 
                    ? profesorFilter 
                    : $"{filteredParams.Filter};{profesorFilter}";
            }
            return filteredParams;
        }

        // Estudiante solo puede ver sus propias notas
        if (User.IsEstudiante())
        {
            var estudianteId = User.GetEstudianteId();
            if (estudianteId.HasValue)
            {
                // Agregar filtro por IdEstudiante
                var estudianteFilter = $"IdEstudiante={estudianteId.Value}";
                filteredParams.Filter = string.IsNullOrEmpty(filteredParams.Filter) 
                    ? estudianteFilter 
                    : $"{filteredParams.Filter};{estudianteFilter}";
            }
            return filteredParams;
        }

        // Si no tiene ningún rol reconocido, no puede ver nada
        throw new UnauthorizedAccessException("No tiene permisos para acceder a las notas");
    }

    /// <summary>
    /// Valida si el usuario puede acceder a una nota específica
    /// </summary>
    private async Task<bool> CanAccessNotaAsync(int notaId)
    {
        try
        {
            var nota = await _service.GetByIdAsync(notaId);
            if (nota == null) return false;

            return User.CanViewNota(nota.IdProfesor, nota.IdEstudiante);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Valida si el usuario puede modificar una nota específica
    /// </summary>
    private async Task<bool> CanModifyNotaAsync(int notaId)
    {
        try
        {
            var nota = await _service.GetByIdAsync(notaId);
            if (nota == null) return false;

            return User.CanManageNotas(nota.IdProfesor);
        }
        catch
        {
            return false;
        }
    }
}

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
/// API endpoints para gestión de Estudiantes.
/// Proporciona operaciones CRUD completas.
/// Admin: acceso completo, Profesor: solo lectura, Estudiante: solo sus datos
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[SwaggerTag("Gestión completa de estudiantes con permisos por rol (CRUD, búsqueda, paginación)")]
public partial class EstudiantesController : ControllerBase
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
    /// Solo Admin y Profesores pueden ver la lista completa.
    /// </summary>
    /// <returns>Lista de estudiantes</returns>
    /// <response code="200">Operación exitosa</response>
    /// <response code="401">No autenticado</response>
    /// <response code="403">Sin permisos para ver todos los estudiantes</response>
    [HttpGet]
    [Authorize]
    [SwaggerOperation(
        Summary = "Obtener todos los estudiantes",
        Description = "Obtiene una lista paginada de estudiantes con filtrado opcional. Solo Admin y Profesores tienen acceso.",
        OperationId = "GetAllStudents",
        Tags = new[] { "Estudiantes" }
    )]
    [SwaggerResponse(200, "Lista de estudiantes obtenida correctamente", typeof(PagedResult<EstudianteDto>))]
    [SwaggerResponse(401, "Token JWT no válido o ausente")]
    [SwaggerResponse(403, "Sin permisos para ver la lista completa de estudiantes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<EstudianteDto>>> GetAll([FromQuery] AudiSoft.School.Application.Common.QueryParams queryParams)
    {
        _logger.LogInformation("Obteniendo todos los estudiantes");
        // Aplicar restricciones por rol: Admin/Profesor ven lista completa, Estudiante solo su propio registro
        var filteredParams = ApplyUserBasedStudentFilters(queryParams);
        var result = await _service.GetPagedAsync(filteredParams);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(result);
    }

    /// <summary>
    /// Búsqueda avanzada de estudiantes mediante expresión en QueryParams.Filter.
    /// Solo Admin y Profesores pueden buscar estudiantes.
    /// </summary>
    [HttpGet("search")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<EstudianteDto>>> Search([FromQuery] AudiSoft.School.Application.Common.QueryParams queryParams)
    {
        _logger.LogInformation("Buscando estudiantes con parámetros: {QueryParams}", queryParams);
        // Mantener seguridad: Estudiante solo puede verse a sí mismo también en búsquedas
        var filteredParams = ApplyUserBasedStudentFilters(queryParams);
        var result = await _service.GetPagedAsync(filteredParams);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un estudiante por su ID.
    /// Admin/Profesor: cualquier estudiante, Estudiante: solo sus propios datos
    /// </summary>
    /// <param name="id">ID del estudiante</param>
    /// <returns>Datos del estudiante</returns>
    /// <response code="200">Estudiante encontrado</response>
    /// <response code="401">No autenticado</response>
    /// <response code="403">Sin permisos para ver este estudiante</response>
    /// <response code="404">Estudiante no encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstudianteDto>> GetById(int id)
    {
        _logger.LogInformation("Obteniendo estudiante con ID {EstudianteId}", id);
        
        // Validar autorización
        if (!User.CanAccessEstudiante(id))
        {
            _logger.LogWarning("Usuario {UserName} intentó acceder a estudiante {EstudianteId} sin permisos", 
                User.GetUserName(), id);
            return Forbid("No tiene permisos para ver este estudiante");
        }
        
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
    /// Solo Admin puede crear estudiantes.
    /// </summary>
    /// <param name="dto">Datos del estudiante a crear</param>
    /// <returns>Estudiante creado</returns>
    /// <response code="201">Estudiante creado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="403">Sin permisos para crear estudiantes</response>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    /// Solo Admin puede actualizar estudiantes.
    /// </summary>
    /// <param name="id">ID del estudiante</param>
    /// <param name="dto">Datos actualizados</param>
    /// <returns>Estudiante actualizado</returns>
    /// <response code="200">Estudiante actualizado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="403">Sin permisos para actualizar estudiantes</response>
    /// <response code="404">Estudiante no encontrado</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstudianteDto>> Update(int id, [FromBody] UpdateEstudianteDto dto)
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
    /// Solo Admin puede eliminar estudiantes.
    /// </summary>
    /// <param name="id">ID del estudiante</param>
    /// <returns>Sin contenido</returns>
    /// <response code="204">Estudiante eliminado exitosamente</response>
    /// <response code="403">Sin permisos para eliminar estudiantes</response>
    /// <response code="404">Estudiante no encontrado</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    
    /// <summary>
    /// Aplica filtros basados en el rol del usuario para restringir acceso a estudiantes
    /// </summary>
    private AudiSoft.School.Application.Common.QueryParams ApplyUserBasedStudentFilters(AudiSoft.School.Application.Common.QueryParams original)
    {
        var filtered = new AudiSoft.School.Application.Common.QueryParams
        {
            Page = original.Page,
            PageSize = original.PageSize,
            MaxPageSize = original.MaxPageSize,
            SortField = original.SortField,
            SortDesc = original.SortDesc,
            Filter = original.Filter,
            FilterField = original.FilterField,
            FilterValue = original.FilterValue
        };

        if (User.IsAdmin() || User.IsProfesor())
        {
            return filtered; // Sin restricciones adicionales
        }

        if (User.IsEstudiante())
        {
            var estudianteId = User.GetEstudianteId();
            if (estudianteId.HasValue)
            {
                var onlySelf = $"Id={estudianteId.Value}";
                filtered.Filter = string.IsNullOrEmpty(filtered.Filter) ? onlySelf : $"{filtered.Filter};{onlySelf}";
            }
            return filtered;
        }

        // Si no tiene un rol reconocido, negar acceso
        throw new UnauthorizedAccessException("No tiene permisos para acceder a estudiantes");
    }
}

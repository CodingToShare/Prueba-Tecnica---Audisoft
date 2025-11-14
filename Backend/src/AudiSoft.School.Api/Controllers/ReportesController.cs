using AudiSoft.School.Application.Common;
using AudiSoft.School.Application.DTOs.Reports;
using AudiSoft.School.Application.Extensions;
using AudiSoft.School.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AudiSoft.School.Api.Controllers;

/// <summary>
/// Endpoints de reportes (resumen y exportación) con filtrado por rol.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[SwaggerTag("Reportes de Notas: resumen estadístico y exportación a CSV")] 
public class ReportesController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(IReportService reportService, ILogger<ReportesController> logger)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene un resumen de notas (total, promedio general, promedios por profesor/estudiante y distribución por rangos).
    /// El resultado se filtra por rol (Admin ve todo; Profesor sus notas; Estudiante sus notas).
    /// </summary>
    [HttpGet("notas/resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<NotasReportSummaryDto>> GetNotasResumen([FromQuery] QueryParams queryParams, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int? idProfesor = null, [FromQuery] int? idEstudiante = null)
    {
        var filtered = BuildRoleAwareQueryParams(queryParams, from, to, idProfesor, idEstudiante);
        var result = await _reportService.GetNotasSummaryAsync(filtered);
        return Ok(result);
    }

    /// <summary>
    /// Exporta notas a CSV con los filtros aplicados por rol y parámetros.
    /// </summary>
    [HttpGet("notas/export")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportNotasCsv([FromQuery] QueryParams queryParams, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int? idProfesor = null, [FromQuery] int? idEstudiante = null)
    {
        var filtered = BuildRoleAwareQueryParams(queryParams, from, to, idProfesor, idEstudiante);
        var csv = await _reportService.ExportNotasCsvAsync(filtered);
        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv)).ToArray();
        return File(bytes, "text/csv", $"notas_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    private QueryParams BuildRoleAwareQueryParams(QueryParams original, DateTime? from, DateTime? to, int? idProfesor, int? idEstudiante)
    {
        var qp = new QueryParams
        {
            Page = 1,
            PageSize = Math.Max(1, original.MaxPageSize), // export uses MaxPageSize as limit
            MaxPageSize = Math.Max(1000, original.MaxPageSize),
            SortField = string.IsNullOrWhiteSpace(original.SortField) ? "CreatedAt" : original.SortField,
            SortDesc = true,
            Filter = original.Filter,
            FilterField = original.FilterField,
            FilterValue = original.FilterValue
        };

        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(qp.Filter)) filters.Add(qp.Filter);

        // Date range
        if (from.HasValue) filters.Add($"CreatedAt>={from.Value:yyyy-MM-dd}");
        if (to.HasValue) filters.Add($"CreatedAt<={to.Value:yyyy-MM-dd}");

        // Explicit filters (Admin only has effect; others will be overridden by role)
        if (idProfesor.HasValue) filters.Add($"IdProfesor={idProfesor.Value}");
        if (idEstudiante.HasValue) filters.Add($"IdEstudiante={idEstudiante.Value}");

        // Role-based constraints
        if (User.IsProfesor())
        {
            var pid = User.GetProfesorId();
            if (pid.HasValue) filters.Add($"IdProfesor={pid.Value}");
        }
        else if (User.IsEstudiante())
        {
            var eid = User.GetEstudianteId();
            if (eid.HasValue) filters.Add($"IdEstudiante={eid.Value}");
        }

        qp.Filter = string.Join(';', filters.Where(f => !string.IsNullOrWhiteSpace(f)));
        return qp;
    }
}

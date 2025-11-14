using System.Globalization;
using System.Text;
using AudiSoft.School.Application.Common;
using AudiSoft.School.Application.DTOs.Reports;
using AudiSoft.School.Application.Extensions;
using AudiSoft.School.Application.Interfaces;
using AudiSoft.School.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AudiSoft.School.Application.Services;

public class ReportService : IReportService
{
    private readonly INotaRepository _notaRepository;

    public ReportService(INotaRepository notaRepository)
    {
        _notaRepository = notaRepository ?? throw new ArgumentNullException(nameof(notaRepository));
    }

    public async Task<NotasReportSummaryDto> GetNotasSummaryAsync(QueryParams queryParams)
    {
        var query = _notaRepository.Query().AsNoTracking().Cast<Nota>();
        query = query.Include(n => n.Profesor).Include(n => n.Estudiante);

        query = query.ApplyFilter(queryParams.Filter, queryParams.FilterField, queryParams.FilterValue);

        var totalNotas = await query.CountAsync();
        decimal promedioGeneral = 0m;
        if (totalNotas > 0)
        {
            promedioGeneral = Math.Round(await query.AverageAsync(n => n.Valor), 2);
        }

        // Promedio por Profesor
        var porProfesor = await query
            .GroupBy(n => new { n.IdProfesor, n.Profesor!.Nombre })
            .Select(g => new GrupoPromedioDto
            {
                Id = g.Key.IdProfesor,
                Nombre = g.Key.Nombre,
                Total = g.Count(),
                Promedio = Math.Round(g.Average(x => x.Valor), 2)
            })
            .OrderByDescending(x => x.Total)
            .Take(10)
            .ToListAsync();

        // Promedio por Estudiante
        var porEstudiante = await query
            .GroupBy(n => new { n.IdEstudiante, n.Estudiante!.Nombre })
            .Select(g => new GrupoPromedioDto
            {
                Id = g.Key.IdEstudiante,
                Nombre = g.Key.Nombre,
                Total = g.Count(),
                Promedio = Math.Round(g.Average(x => x.Valor), 2)
            })
            .OrderByDescending(x => x.Total)
            .Take(10)
            .ToListAsync();

        // Distribución por rangos
        var rangos = new (string Label, decimal Min, decimal Max)[]
        {
            ("0-59", 0m, 59.9999m),
            ("60-69", 60m, 69.9999m),
            ("70-79", 70m, 79.9999m),
            ("80-89", 80m, 89.9999m),
            ("90-100", 90m, 100m)
        };

        var distribucion = new List<RangoDistribucionDto>();
        foreach (var r in rangos)
        {
            var count = await query.Where(n => n.Valor >= r.Min && n.Valor <= r.Max).CountAsync();
            distribucion.Add(new RangoDistribucionDto { Rango = r.Label, Conteo = count });
        }

        return new NotasReportSummaryDto
        {
            TotalNotas = totalNotas,
            PromedioGeneral = promedioGeneral,
            PromedioPorProfesor = porProfesor,
            PromedioPorEstudiante = porEstudiante,
            DistribucionPorRangos = distribucion
        };
    }

    public async Task<string> ExportNotasCsvAsync(QueryParams queryParams)
    {
        var query = _notaRepository.Query().AsNoTracking().Cast<Nota>();
        query = query.Include(n => n.Profesor).Include(n => n.Estudiante);
        query = query.ApplyFilter(queryParams.Filter, queryParams.FilterField, queryParams.FilterValue)
                     .ApplySorting(queryParams.SortField, queryParams.SortDesc);

        // Limit export size sensibly (could be made configurable)
        var items = await query.Take(Math.Min(queryParams.MaxPageSize > 0 ? queryParams.MaxPageSize : 1000, 5000)).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Id,Nombre,Valor,Profesor,Estudiante,CreatedAt");
        foreach (var n in items)
        {
            var line = string.Join(',', new[]
            {
                n.Id.ToString(),
                Escape(n.Nombre),
                n.Valor.ToString(CultureInfo.InvariantCulture),
                Escape(n.Profesor?.Nombre ?? string.Empty),
                Escape(n.Estudiante?.Nombre ?? string.Empty),
                (n.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty)
            });
            sb.AppendLine(line);
        }

        return sb.ToString();
    }

    private static string Escape(string input)
    {
        if (input.Contains('"') || input.Contains(',') || input.Contains('\n'))
        {
            return '"' + input.Replace("\"", "\"\"") + '"';
        }
        return input;
    }
}

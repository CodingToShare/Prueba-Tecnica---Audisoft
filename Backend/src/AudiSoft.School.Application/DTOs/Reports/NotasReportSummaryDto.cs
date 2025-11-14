using System.Collections.Generic;

namespace AudiSoft.School.Application.DTOs.Reports;

public class NotasReportSummaryDto
{
    public int TotalNotas { get; set; }
    public decimal PromedioGeneral { get; set; }

    public List<GrupoPromedioDto> PromedioPorProfesor { get; set; } = new();
    public List<GrupoPromedioDto> PromedioPorEstudiante { get; set; } = new();

    public List<RangoDistribucionDto> DistribucionPorRangos { get; set; } = new();
}

public class GrupoPromedioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Total { get; set; }
    public decimal Promedio { get; set; }
}

public class RangoDistribucionDto
{
    public string Rango { get; set; } = string.Empty; // e.g., "0-59"
    public int Conteo { get; set; }
}

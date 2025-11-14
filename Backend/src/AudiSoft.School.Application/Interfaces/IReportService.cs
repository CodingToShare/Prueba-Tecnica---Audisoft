using System.Threading.Tasks;
using AudiSoft.School.Application.Common;
using AudiSoft.School.Application.DTOs.Reports;

namespace AudiSoft.School.Application.Interfaces;

public interface IReportService
{
    Task<NotasReportSummaryDto> GetNotasSummaryAsync(QueryParams queryParams);
    Task<string> ExportNotasCsvAsync(QueryParams queryParams);
}

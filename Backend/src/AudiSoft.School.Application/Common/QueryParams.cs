namespace AudiSoft.School.Application.Common;

/// <summary>
/// Parámetros comunes para consultas: filtrado, ordenamiento y paginación.
/// </summary>
public class QueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    // Tamaño máximo permitido por página. Se aplicará en ApplyPagingAsync para evitar peticiones excesivas.
    public int MaxPageSize { get; set; } = 100;

    // Campo por el que filtrar (nombre de propiedad simple)
    public string? FilterField { get; set; }
    public string? FilterValue { get; set; }

    // Expresión de filtro avanzada. Ejemplos:
    //  - "Nombre:Juan" (contains)
    //  - "Id=5" (equals)
    //  - "Valor>50;Nombre:Maria" (AND)
    //  - "Nombre:Juan|Nombre:Maria" (OR)
    public string? Filter { get; set; }

    // Ordenamiento
    public string? SortField { get; set; }
    public bool SortDesc { get; set; } = false;
}

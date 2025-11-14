namespace AudiSoft.School.Application.Common;

/// <summary>
/// Parámetros comunes para consultas: filtrado, ordenamiento y paginación.
/// Soporta filtrado simple y avanzado con múltiples operadores.
/// </summary>
public class QueryParams
{
    /// <summary>
    /// Número de página a obtener (empezando en 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Cantidad de elementos por página (máximo definido por MaxPageSize)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Tamaño máximo permitido por página para evitar peticiones excesivas
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Campo por el que filtrar usando filtrado simple (nombre de propiedad)
    /// </summary>
    public string? FilterField { get; set; }

    /// <summary>
    /// Valor a buscar en el campo especificado en FilterField
    /// </summary>
    public string? FilterValue { get; set; }

    /// <summary>
    /// Expresión de filtro avanzada. Ejemplos:
    /// - "Nombre:Juan" (contains)
    /// - "Id=5" (equals) 
    /// - "Valor>50;Nombre:Maria" (AND)
    /// - "Nombre:Juan|Nombre:Maria" (OR)
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// Campo por el cual ordenar los resultados (nombre de propiedad)
    /// </summary>
    public string? SortField { get; set; }

    /// <summary>
    /// Indica si el ordenamiento debe ser descendente (true) o ascendente (false)
    /// </summary>
    public bool SortDesc { get; set; } = false;
}

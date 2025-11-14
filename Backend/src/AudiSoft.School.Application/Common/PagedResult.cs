namespace AudiSoft.School.Application.Common;

/// <summary>
/// Resultado paginado genérico que contiene elementos y metadatos de paginación.
/// </summary>
/// <typeparam name="T">Tipo de elementos en la colección paginada</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Elementos de la página actual
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// Número total de elementos en toda la consulta (sin paginación)
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Número de página actual
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Cantidad de elementos por página
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Número total de páginas calculado automáticamente
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Indica si existe una página anterior
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Indica si existe una página siguiente
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}

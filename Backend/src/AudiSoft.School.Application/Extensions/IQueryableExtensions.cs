using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using AudiSoft.School.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace AudiSoft.School.Application.Extensions;

/// <summary>
/// Extensiones para IQueryable que proporcionan filtrado dinámico, ordenamiento y paginación.
/// Soporta expresiones complejas como "Nombre:Juan;Valor>50|Nombre:Maria"
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    /// Aplica filtros dinámicos basados en una expresión de filtro avanzada o filtro simple de campo/valor.
    /// </summary>
    /// <param name="query">Query base</param>
    /// <param name="filterExpression">Expresión avanzada como "Nombre:Juan;Id>5|Nombre:Maria"</param>
    /// <param name="field">Campo simple para filtrar (usado si filterExpression está vacío)</param>
    /// <param name="value">Valor simple para filtrar (usado si filterExpression está vacío)</param>
    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, string? filterExpression, string? field, string? value)
    {
        if (!string.IsNullOrWhiteSpace(filterExpression))
        {
            return query.ApplyAdvancedFilter(filterExpression);
        }

        return query.ApplySimpleFilter(field, value);
    }

    /// <summary>
    /// Aplica filtros avanzados con sintaxis: "Campo:Valor" (contains), "Campo=Valor" (equals), "Campo>Valor", etc.
    /// Soporta operadores lógicos: ';' para AND, '|' para OR
    /// </summary>
    private static IQueryable<T> ApplyAdvancedFilter<T>(this IQueryable<T> query, string filterExpression)
    {
        var orGroups = filterExpression.Split('|', StringSplitOptions.RemoveEmptyEntries);
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? orExpression = null;

        foreach (var group in orGroups)
        {
            var andParts = group.Split(';', StringSplitOptions.RemoveEmptyEntries);
            Expression? andExpression = null;

            foreach (var part in andParts)
            {
                var trimmedPart = part.Trim();
                if (string.IsNullOrWhiteSpace(trimmedPart))
                    continue;

                var condition = BuildFilterCondition<T>(parameter, trimmedPart);
                if (condition == null)
                    continue;

                andExpression = andExpression == null ? condition : Expression.AndAlso(andExpression, condition);
            }

            if (andExpression != null)
            {
                orExpression = orExpression == null ? andExpression : Expression.OrElse(orExpression, andExpression);
            }
        }

        if (orExpression == null)
            return query;

        var lambda = Expression.Lambda<Func<T, bool>>(orExpression, parameter);
        return query.Where(lambda);
    }

    /// <summary>
    /// Construye una condición de filtro a partir de un token como "Nombre:Juan" o "Valor>50"
    /// </summary>
    private static Expression? BuildFilterCondition<T>(ParameterExpression parameter, string filterPart)
    {
        // Regex para capturar: campo operador valor (soporta propiedades anidadas)
        var match = Regex.Match(filterPart, @"^\s*(\w+(?:\.\w+)*)\s*(==|=|!=|>=|<=|>|<|:)\s*(.+)\s*$");
        
        string propName, op, value;
        if (match.Success)
        {
            propName = match.Groups[1].Value;
            op = match.Groups[2].Value;
            value = match.Groups[3].Value.Trim();
        }
        else
        {
            // Fallback para formato simple "Campo:Valor"
            var separatorIndex = filterPart.IndexOfAny(new[] { ':', '=' });
            if (separatorIndex <= 0) return null;
            
            propName = filterPart.Substring(0, separatorIndex).Trim();
            op = filterPart[separatorIndex].ToString();
            value = filterPart.Substring(separatorIndex + 1).Trim();
        }

        // Construir acceso a propiedad (soporta propiedades anidadas como "Profesor.Nombre")
        var (memberExpression, propType) = BuildPropertyAccess<T>(parameter, propName);
        if (memberExpression == null || propType == null) return null;

        return BuildComparisonExpression(memberExpression, propType, op, value);
    }

    /// <summary>
    /// Construye expresión de acceso a propiedad, incluyendo propiedades anidadas
    /// </summary>
    private static (Expression? memberExpression, Type? propType) BuildPropertyAccess<T>(ParameterExpression parameter, string propertyPath)
    {
        var parts = propertyPath.Split('.', StringSplitOptions.RemoveEmptyEntries);
        Type currentType = typeof(T);
        Expression currentExpr = parameter;

        foreach (var part in parts)
        {
            var prop = currentType.GetProperty(part, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                return (null, null);

            currentExpr = Expression.Property(currentExpr, prop);
            currentType = prop.PropertyType;
        }

        return (currentExpr, currentType);
    }

    /// <summary>
    /// Construye expresión de comparación basada en el tipo de dato y operador
    /// </summary>
    private static Expression? BuildComparisonExpression(Expression memberExpression, Type propType, string op, string value)
    {
        // Manejar tipos string
        if (propType == typeof(string))
        {
            var constExpr = Expression.Constant(value, typeof(string));
            
            return op switch
            {
                ":" => Expression.Call(memberExpression, typeof(string).GetMethod("Contains", new[] { typeof(string) })!, constExpr),
                "=" or "==" => Expression.Call(memberExpression, typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(StringComparison) })!, 
                                              constExpr, Expression.Constant(StringComparison.OrdinalIgnoreCase)),
                "!=" => Expression.Not(Expression.Call(memberExpression, typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(StringComparison) })!, 
                                                     constExpr, Expression.Constant(StringComparison.OrdinalIgnoreCase))),
                _ => null
            };
        }

        // Manejar tipos numéricos y fechas
        try
        {
            object? parsedValue = TryParseValue(value, propType);
            if (parsedValue == null) return null;

            var constant = Expression.Constant(parsedValue, propType);
            var convertedConstant = Expression.Convert(constant, propType);

            return op switch
            {
                "=" or "==" => Expression.Equal(memberExpression, convertedConstant),
                "!=" => Expression.NotEqual(memberExpression, convertedConstant),
                ">" => Expression.GreaterThan(memberExpression, convertedConstant),
                "<" => Expression.LessThan(memberExpression, convertedConstant),
                ">=" => Expression.GreaterThanOrEqual(memberExpression, convertedConstant),
                "<=" => Expression.LessThanOrEqual(memberExpression, convertedConstant),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Intenta parsear un valor string al tipo de dato especificado
    /// </summary>
    private static object? TryParseValue(string value, Type targetType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            return underlyingType switch
            {
                Type t when t == typeof(int) => int.Parse(value),
                Type t when t == typeof(long) => long.Parse(value),
                Type t when t == typeof(decimal) => decimal.Parse(value),
                Type t when t == typeof(double) => double.Parse(value),
                Type t when t == typeof(DateTime) => DateTime.Parse(value),
                Type t when t.IsEnum => Enum.Parse(t, value, true),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Aplica filtro simple basado en campo y valor únicos
    /// </summary>
    private static IQueryable<T> ApplySimpleFilter<T>(this IQueryable<T> query, string? field, string? value)
    {
        if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(value))
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var (memberExpression, propType) = BuildPropertyAccess<T>(parameter, field);
        
        if (memberExpression == null || propType == null)
            return query;

        Expression? condition = null;

        // Para strings, usar Contains por defecto (o equals si empieza con '=')
        if (propType == typeof(string))
        {
            var isExact = value.StartsWith("=");
            var actualValue = isExact ? value.Substring(1) : value;
            var constant = Expression.Constant(actualValue, typeof(string));

            if (isExact)
            {
                condition = Expression.Call(memberExpression, 
                    typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(StringComparison) })!,
                    constant, Expression.Constant(StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                condition = Expression.Call(memberExpression, 
                    typeof(string).GetMethod("Contains", new[] { typeof(string) })!, constant);
            }
        }
        else
        {
            // Para tipos numéricos, intentar igualdad exacta
            var parsedValue = TryParseValue(value, propType);
            if (parsedValue != null)
            {
                var constant = Expression.Constant(parsedValue, propType);
                condition = Expression.Equal(memberExpression, Expression.Convert(constant, propType));
            }
            else
            {
                // Fallback: convertir a string y usar Contains
                var toStringMethod = propType.GetMethod("ToString", Type.EmptyTypes);
                if (toStringMethod != null)
                {
                    var memberAsString = Expression.Call(memberExpression, toStringMethod);
                    var constant = Expression.Constant(value, typeof(string));
                    condition = Expression.Call(memberAsString, 
                        typeof(string).GetMethod("Contains", new[] { typeof(string) })!, constant);
                }
            }
        }

        if (condition == null) return query;

        var lambda = Expression.Lambda<Func<T, bool>>(condition, parameter);
        return query.Where(lambda);
    }

    /// <summary>
    /// Aplica ordenamiento dinámico por campo especificado
    /// </summary>
    /// <param name="query">Query base</param>
    /// <param name="sortField">Campo por el que ordenar (soporta propiedades anidadas)</param>
    /// <param name="desc">true para orden descendente, false para ascendente</param>
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortField, bool desc)
    {
        if (string.IsNullOrWhiteSpace(sortField))
            return query;

        // Soportar propiedades anidadas
        var parameter = Expression.Parameter(typeof(T), "x");
        var (memberExpression, propType) = BuildPropertyAccess<T>(parameter, sortField);
        
        if (memberExpression == null || propType == null)
            return query;

        var keySelector = Expression.Lambda(memberExpression, parameter);
        var methodName = desc ? "OrderByDescending" : "OrderBy";
        
        var resultExp = Expression.Call(typeof(Queryable), methodName, 
            new Type[] { typeof(T), propType }, 
            query.Expression, Expression.Quote(keySelector));
            
        return query.Provider.CreateQuery<T>(resultExp);
    }

    /// <summary>
    /// Aplica paginación y proyección a una consulta
    /// </summary>
    /// <param name="query">Query base</param>
    /// <param name="queryParams">Parámetros de paginación</param>
    /// <param name="projector">Función de proyección de entidad a DTO</param>
    public static async Task<PagedResult<TDestination>> ApplyPagingAsync<TSource, TDestination>(
        this IQueryable<TSource> query, 
        QueryParams queryParams, 
        Func<TSource, TDestination> projector)
    {
        // Aplicar límites sensatos al tamaño de página
        var pageSize = Math.Min(Math.Max(1, queryParams.PageSize), Math.Max(1, queryParams.MaxPageSize));
        var page = Math.Max(1, queryParams.Page);
        
        var total = await query.CountAsync();
        var skip = (page - 1) * pageSize;
        
        var items = await query.Skip(skip).Take(pageSize).ToListAsync();
        var projected = items.Select(projector).ToList();
        
        return new PagedResult<TDestination>
        {
            Items = projected,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }
}

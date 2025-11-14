using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using AudiSoft.School.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace AudiSoft.School.Application.Extensions;

public static class IQueryableExtensions
{
    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, string? filterExpression, string? field, string? value)
    {
        // If an advanced filter expression is provided, parse it; otherwise fallback to simple field/value filtering
        if (!string.IsNullOrWhiteSpace(filterExpression))
        {
            // Support OR groups separated by '|', AND within group separated by ';'
            // Example: "Nombre:Juan;Valor>50|Nombre:Maria"
            var orGroups = filterExpression.Split('|', StringSplitOptions.RemoveEmptyEntries);
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            Expression? orExpression = null;

            foreach (var group in orGroups)
            {
                var andParts = group.Split(';', StringSplitOptions.RemoveEmptyEntries);
                Expression? andExpression = null;

                foreach (var partRaw in andParts)
                {
                    var part = partRaw.Trim();
                    if (string.IsNullOrWhiteSpace(part))
                        continue;

                    // Parse token: field operator value
                    var m = Regex.Match(part, "^\\s*(\\w+)\\s*(==|=|!=|>=|<=|>|<|:)\\s*(.+)\\s*$");
                    string propName;
                    string op;
                    string val;

                    if (m.Success)
                    {
                        propName = m.Groups[1].Value;
                        op = m.Groups[2].Value;
                        val = m.Groups[3].Value.Trim();
                    }
                    else
                    {
                        // Fallback: try "Field:Value" or "Field Value"
                        var idx = part.IndexOfAny(new[] { ':', '=' });
                        if (idx > 0)
                        {
                            propName = part.Substring(0, idx).Trim();
                            op = part[idx].ToString();
                            val = part.Substring(idx + 1).Trim();
                        }
                        else
                        {
                            // No operator: assume contains on string property with entire part as value and unknown field -> skip
                            continue;
                        }
                    }

                    var prop = typeof(T).GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (prop == null)
                        continue;
                        // Support nested properties using dot notation: "Profesor.Nombre"
                        var propInfo = typeof(T).GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                        Expression member = parameter;
                        Type propType = typeof(T);
                        if (propName.Contains('.'))
                        {
                            var parts = propName.Split('.', StringSplitOptions.RemoveEmptyEntries);
                            Type currentType = typeof(T);
                            Expression currentExpr = parameter;
                            PropertyInfo? currentProp = null;
                            bool failed = false;
                            foreach (var part in parts)
                            {
                                currentProp = currentType.GetProperty(part, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                                if (currentProp == null)
                                {
                                    failed = true;
                                    break;
                                }
                                currentExpr = Expression.Property(currentExpr, currentProp);
                                currentType = currentProp.PropertyType;
                            }
                            if (failed)
                                continue;
                            member = currentExpr;
                            propType = currentType;
                        }
                        else
                        {
                            var prop = propInfo ?? typeof(T).GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (prop == null)
                                continue;
                            member = Expression.Property(parameter, prop);
                            propType = prop.PropertyType;
                        }
                    var member = Expression.Property(parameter, prop);
                    var propType = prop.PropertyType;
                    Expression? condition = null;

                    // Handle string operations
                    if (propType == typeof(string))
                    {
                        var constExpr = Expression.Constant(val, typeof(string));
                        if (op == ":")
                        {
                            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                            condition = Expression.Call(member, method!, constExpr);
                        }
                        else if (op == "=" || op == "==")
                        {
                            var method = typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(StringComparison) });
                            var comparison = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                            condition = Expression.Call(member, method!, constExpr, comparison);
                        }
                        else if (op == "!=")
                        {
                            var method = typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(StringComparison) });
                            var comparison = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                            var equalsCall = Expression.Call(member, method!, constExpr, comparison);
                            condition = Expression.Not(equalsCall);
                        }
                    }
                    else
                    {
                        // numeric or comparable types
                        // attempt to parse value into target type
                        try
                        {
                            object? parsed = null;
                            if (propType == typeof(int) || propType == typeof(int?))
                                parsed = int.Parse(val);
                            else if (propType == typeof(long) || propType == typeof(long?))
                                parsed = long.Parse(val);
                            else if (propType == typeof(decimal) || propType == typeof(decimal?))
                                parsed = decimal.Parse(val);
                            else if (propType == typeof(double) || propType == typeof(double?))
                                parsed = double.Parse(val);
                            else if (propType == typeof(DateTime) || propType == typeof(DateTime?))
                                parsed = DateTime.Parse(val);
                            else if (propType.IsEnum)
                                parsed = Enum.Parse(propType, val, true);

                            if (parsed != null)
                            {
                                var constant = Expression.Constant(parsed, propType);
                                switch (op)
                                {
                                    case "=":
                                    case "==":
                                        condition = Expression.Equal(member, Expression.Convert(constant, propType));
                                        break;
                                    case "!=":
                                        condition = Expression.NotEqual(member, Expression.Convert(constant, propType));
                                        break;
                                    case ">":
                                        condition = Expression.GreaterThan(member, Expression.Convert(constant, propType));
                                        break;
                                    case "<":
                                        condition = Expression.LessThan(member, Expression.Convert(constant, propType));
                                        break;
                                    case ">=":
                                        condition = Expression.GreaterThanOrEqual(member, Expression.Convert(constant, propType));
                                        break;
                                    case "<=":
                                        condition = Expression.LessThanOrEqual(member, Expression.Convert(constant, propType));
                                        break;
                                }
                            }
                        }
                        catch
                        {
                            // ignore parse errors for this token
                        }
                    }

                    if (condition == null)
                        continue;

                    if (andExpression == null)
                        andExpression = condition;
                    else
                        andExpression = Expression.AndAlso(andExpression, condition);
                }

                if (andExpression != null)
                {
                    if (orExpression == null)
                        orExpression = andExpression;
                    else
                        orExpression = Expression.OrElse(orExpression, andExpression);
                }
            }

            if (orExpression == null)
                return query;

            var lambda = Expression.Lambda<Func<T, bool>>(orExpression, parameter);
            return query.Where(lambda);
        }

        // Fallback to original simple field/value behavior
        if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(value))
            // Support nested property for simple field/value fallback
            PropertyInfo? propSimple = null;
            Expression parameterSimple = Expression.Parameter(typeof(T), "x");
            Expression memberSimpleExpr = parameterSimple;
            var propTypeSimple = typeof(object);
            if (field.Contains('.'))
            {
                var parts = field.Split('.', StringSplitOptions.RemoveEmptyEntries);
                Type currentType = typeof(T);
                Expression currentExpr = parameterSimple;
                PropertyInfo? currentProp = null;
                bool failed = false;
                foreach (var part in parts)
                {
                    currentProp = currentType.GetProperty(part, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (currentProp == null)
                    {
                        failed = true;
                        break;
                    }
                    currentExpr = Expression.Property(currentExpr, currentProp);
                    currentType = currentProp.PropertyType;
                }
                if (failed)
                    return query;
                memberSimpleExpr = currentExpr;
                propTypeSimple = currentType;
            }
            else
            {
                propSimple = typeof(T).GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propSimple == null)
                    return query;
                memberSimpleExpr = Expression.Property(parameterSimple, propSimple);
                propTypeSimple = propSimple.PropertyType;
            }
        var propSimple = typeof(T).GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (propSimple == null)
            return query;

        var parameterSimple = Expression.Parameter(typeof(T), "x");
        var memberSimple = Expression.Property(parameterSimple, propSimple);

        var propTypeSimple = propSimple.PropertyType;
        if (propTypeSimple == typeof(int) || propTypeSimple == typeof(int?) ||
            propTypeSimple == typeof(long) || propTypeSimple == typeof(long?) ||
            propTypeSimple == typeof(decimal) || propTypeSimple == typeof(decimal?) ||
            propTypeSimple == typeof(double) || propTypeSimple == typeof(double?))
        {
            try
            {
                Expression? equality = null;
                if (propTypeSimple == typeof(int) || propTypeSimple == typeof(int?))
                {
                    if (int.TryParse(value, out var intVal))
                    {
                        var constant = Expression.Constant(intVal, typeof(int));
                        equality = Expression.Equal(memberSimple, Expression.Convert(constant, propTypeSimple));
                    }
                }
                else if (propTypeSimple == typeof(long) || propTypeSimple == typeof(long?))
                {
                    if (long.TryParse(value, out var longVal))
                    {
                        var constant = Expression.Constant(longVal, typeof(long));
                        equality = Expression.Equal(memberSimple, Expression.Convert(constant, propTypeSimple));
                    }
                }
                else if (propTypeSimple == typeof(decimal) || propTypeSimple == typeof(decimal?))
                {
                    if (decimal.TryParse(value, out var decVal))
                    {
                        var constant = Expression.Constant(decVal, typeof(decimal));
                        equality = Expression.Equal(memberSimple, Expression.Convert(constant, propTypeSimple));
                    }
                }
                else if (propTypeSimple == typeof(double) || propTypeSimple == typeof(double?))
                {
                    if (double.TryParse(value, out var dblVal))
                    {
                        var constant = Expression.Constant(dblVal, typeof(double));
                        equality = Expression.Equal(memberSimple, Expression.Convert(constant, propTypeSimple));
                    }
                }

                if (equality != null)
                {
                    var lambda = Expression.Lambda<Func<T, bool>>(equality, parameterSimple);
                    return query.Where(lambda);
                }
            }
            catch
            {
                // ignore parse errors and fall back to string comparison
            }
        }

        if (propTypeSimple == typeof(string))
        {
            var raw = value;
            var isExact = raw.StartsWith("=");
            if (isExact)
                raw = raw.Substring(1);

            var constant = Expression.Constant(raw, typeof(string));
            var memberExpr = memberSimple;
            var method = isExact
                ? typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(StringComparison) })
                : typeof(string).GetMethod("Contains", new[] { typeof(string) });

            Expression call;
            if (isExact && method != null)
            {
                var comparison = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                call = Expression.Call(memberExpr, method, constant, comparison);
            }
            else
            {
                call = Expression.Call(memberExpr, method!, constant);
            }

            var lambda = Expression.Lambda<Func<T, bool>>(call, parameterSimple);
            return query.Where(lambda);
        }

        Expression memberAsString = memberSimple;
        var toStringMethod = propTypeSimple.GetMethod("ToString", Type.EmptyTypes);
        if (toStringMethod != null)
            memberAsString = Expression.Call(memberSimple, toStringMethod);

        var constVal = Expression.Constant(value, typeof(string));
        var containsMethodFallback = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        var containsCallFallback = Expression.Call(memberAsString, containsMethodFallback!, constVal);
        var lambdaFallback = Expression.Lambda<Func<T, bool>>(containsCallFallback, parameterSimple);
        return query.Where(lambdaFallback);
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortField, bool desc)
    {
        if (string.IsNullOrWhiteSpace(sortField))
            return query;

        var prop = typeof(T).GetProperty(sortField, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (prop == null)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var member = Expression.Property(parameter, prop);
        var keySelector = Expression.Lambda(member, parameter);

        var methodName = desc ? "OrderByDescending" : "OrderBy";
        var resultExp = Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(T), prop.PropertyType }, query.Expression, Expression.Quote(keySelector));
        return query.Provider.CreateQuery<T>(resultExp);
    }

    public static async Task<PagedResult<TDestination>> ApplyPagingAsync<TSource, TDestination>(this IQueryable<TSource> query, QueryParams queryParams, Func<TSource, TDestination> projector)
    {
        // Enforce sensible limits on PageSize
        var pageSize = Math.Min(Math.Max(1, queryParams.PageSize), Math.Max(1, queryParams.MaxPageSize));
        var total = await query.CountAsync();
        var skip = (Math.Max(queryParams.Page, 1) - 1) * pageSize;
        var items = await query.Skip(skip).Take(pageSize).ToListAsync();
        var projected = items.Select(projector);
        return new PagedResult<TDestination>
        {
            Items = projected,
            TotalCount = total,
            Page = queryParams.Page,
            PageSize = pageSize
        };
    }
}

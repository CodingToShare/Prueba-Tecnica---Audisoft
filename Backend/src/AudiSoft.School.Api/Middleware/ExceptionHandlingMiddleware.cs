using AudiSoft.School.Domain.Exceptions;
using System.Net;
using System.Text.Json;
using FluentValidation;

namespace AudiSoft.School.Api.Middleware;

/// <summary>
/// Middleware para manejar excepciones globales con logging estructurado.
/// Convierte excepciones en respuestas HTTP estructuradas y registra errores con niveles apropiados.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Invoca el middleware para procesar la solicitud.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Maneja la excepción y retorna una respuesta HTTP apropiada con logging estructurado.
    /// </summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var correlationId = context.TraceIdentifier;
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var userAgent = context.Request.Headers.UserAgent.ToString();

        var (statusCode, logLevel, errorResponse) = exception switch
        {
            EntityNotFoundException ex => (
                StatusCodes.Status404NotFound,
                LogLevel.Warning,
                CreateErrorResponse(StatusCodes.Status404NotFound, ex.Message, correlationId)
            ),
            
            InvalidEntityStateException ex => (
                StatusCodes.Status400BadRequest,
                LogLevel.Warning,
                CreateErrorResponse(StatusCodes.Status400BadRequest, ex.Message, correlationId)
            ),
            
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                LogLevel.Warning,
                CreateValidationErrorResponse(ex, correlationId)
            ),
            
            DuplicateEntityException ex => (
                StatusCodes.Status409Conflict,
                LogLevel.Warning,
                CreateErrorResponse(StatusCodes.Status409Conflict, ex.Message, correlationId)
            ),
            
            BusinessRuleViolationException ex => (
                StatusCodes.Status422UnprocessableEntity,
                LogLevel.Warning,
                CreateErrorResponse(StatusCodes.Status422UnprocessableEntity, ex.Message, correlationId)
            ),
            
            DomainException ex => (
                StatusCodes.Status400BadRequest,
                LogLevel.Warning,
                CreateErrorResponse(StatusCodes.Status400BadRequest, ex.Message, correlationId)
            ),
            
            UnauthorizedAccessException ex => (
                StatusCodes.Status401Unauthorized,
                LogLevel.Warning,
                CreateErrorResponse(StatusCodes.Status401Unauthorized, "No autorizado", correlationId)
            ),
            
            TimeoutException ex => (
                StatusCodes.Status408RequestTimeout,
                LogLevel.Error,
                CreateErrorResponse(StatusCodes.Status408RequestTimeout, "Tiempo de espera agotado", correlationId)
            ),
            
            _ => (
                StatusCodes.Status500InternalServerError,
                LogLevel.Error,
                CreateErrorResponse(StatusCodes.Status500InternalServerError, 
                    "Ha ocurrido un error interno en el servidor", correlationId)
            )
        };

        context.Response.StatusCode = statusCode;

        // Logging estructurado con contexto enriquecido
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = requestPath,
            ["RequestMethod"] = requestMethod,
            ["StatusCode"] = statusCode,
            ["UserAgent"] = userAgent,
            ["ExceptionType"] = exception.GetType().Name
        }))
        {
            if (logLevel == LogLevel.Error)
            {
                _logger.LogError(exception, 
                    "Error no controlado en {RequestMethod} {RequestPath}. CorrelationId: {CorrelationId}",
                    requestMethod, requestPath, correlationId);
            }
            else
            {
                _logger.LogWarning(exception,
                    "Excepción de negocio en {RequestMethod} {RequestPath}. CorrelationId: {CorrelationId}",
                    requestMethod, requestPath, correlationId);
            }
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private ErrorResponse CreateErrorResponse(int statusCode, string message, string correlationId)
    {
        return new ErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            Details = _environment.IsDevelopment() ? GetStatusCodeDescription(statusCode) : null
        };
    }

    private ValidationErrorResponse CreateValidationErrorResponse(ValidationException ex, string correlationId)
    {
        var errors = ex.Errors?.GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            ) ?? new Dictionary<string, string[]>();

        return new ValidationErrorResponse
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "Errores de validación en los datos enviados",
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            ValidationErrors = errors,
            Details = _environment.IsDevelopment() ? "Revise los campos marcados con errores" : null
        };
    }

    private static string GetStatusCodeDescription(int statusCode) => statusCode switch
    {
        400 => "Bad Request - Los datos enviados no son válidos",
        401 => "Unauthorized - Se requiere autenticación",
        403 => "Forbidden - No tiene permisos para realizar esta acción",
        404 => "Not Found - El recurso solicitado no existe",
        408 => "Request Timeout - La solicitud tardó demasiado tiempo",
        409 => "Conflict - Conflicto con el estado actual del recurso",
        422 => "Unprocessable Entity - Los datos son válidos pero no procesables",
        500 => "Internal Server Error - Error interno del servidor",
        _ => "Error en la solicitud"
    };
}

/// <summary>
/// Modelo de respuesta de error estandarizado.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Código de estado HTTP.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Mensaje de error principal.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Identificador de correlación para rastreo.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp UTC de cuando ocurrió el error.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Detalles adicionales (solo en Development).
    /// </summary>
    public string? Details { get; set; }
}

/// <summary>
/// Respuesta especializada para errores de validación.
/// </summary>
public class ValidationErrorResponse : ErrorResponse
{
    /// <summary>
    /// Errores de validación agrupados por campo.
    /// </summary>
    public Dictionary<string, string[]> ValidationErrors { get; set; } = new();
}

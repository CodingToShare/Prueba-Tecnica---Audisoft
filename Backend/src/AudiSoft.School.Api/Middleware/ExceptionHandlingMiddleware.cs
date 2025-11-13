using AudiSoft.School.Domain.Exceptions;
using System.Net;

namespace AudiSoft.School.Api.Middleware;

/// <summary>
/// Middleware para manejar excepciones globales.
/// Convierte excepciones en respuestas HTTP estructuradas.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
            _logger.LogError(ex, "Excepción no manejada ocurrida");
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Maneja la excepción y retorna una respuesta HTTP apropiada.
    /// </summary>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case EntityNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = ex.Message;
                response.StatusCode = StatusCodes.Status404NotFound;
                break;

            case InvalidEntityStateException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = ex.Message;
                response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case DomainException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = ex.Message;
                response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = "Ha ocurrido un error interno en el servidor";
                response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
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
    /// Mensaje de error.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp de cuando ocurrió el error.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

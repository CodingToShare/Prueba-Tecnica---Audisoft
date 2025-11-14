using System.Security.Claims;

namespace AudiSoft.School.Application.Extensions;

/// <summary>
/// Extensiones para trabajar con claims del usuario autenticado
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Obtiene el ID del usuario desde los claims
    /// </summary>
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Usuario no válido o ID no encontrado");
        }
        return userId;
    }

    /// <summary>
    /// Obtiene el nombre de usuario desde los claims
    /// </summary>
    public static string GetUserName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("userName")?.Value ?? 
               principal.FindFirst(ClaimTypes.Name)?.Value ?? 
               throw new UnauthorizedAccessException("Nombre de usuario no encontrado");
    }

    /// <summary>
    /// Obtiene los roles del usuario desde los claims
    /// </summary>
    public static List<string> GetRoles(this ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }

    /// <summary>
    /// Verifica si el usuario tiene un rol específico
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
    {
        return principal.IsInRole(role);
    }

    /// <summary>
    /// Verifica si el usuario tiene alguno de los roles especificados
    /// </summary>
    public static bool HasAnyRole(this ClaimsPrincipal principal, params string[] roles)
    {
        return roles.Any(role => principal.IsInRole(role));
    }

    /// <summary>
    /// Obtiene el ID del profesor si el usuario está asociado a uno
    /// </summary>
    public static int? GetProfesorId(this ClaimsPrincipal principal)
    {
        var profesorIdClaim = principal.FindFirst("idProfesor")?.Value;
        return string.IsNullOrEmpty(profesorIdClaim) ? null : int.Parse(profesorIdClaim);
    }

    /// <summary>
    /// Obtiene el ID del estudiante si el usuario está asociado a uno
    /// </summary>
    public static int? GetEstudianteId(this ClaimsPrincipal principal)
    {
        var estudianteIdClaim = principal.FindFirst("idEstudiante")?.Value;
        return string.IsNullOrEmpty(estudianteIdClaim) ? null : int.Parse(estudianteIdClaim);
    }

    /// <summary>
    /// Verifica si el usuario es administrador
    /// </summary>
    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("Admin");
    }

    /// <summary>
    /// Verifica si el usuario es profesor
    /// </summary>
    public static bool IsProfesor(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("Profesor");
    }

    /// <summary>
    /// Verifica si el usuario es estudiante
    /// </summary>
    public static bool IsEstudiante(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("Estudiante");
    }

    /// <summary>
    /// Obtiene el email del usuario desde los claims
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Verifica si el usuario puede acceder a datos de un profesor específico
    /// </summary>
    public static bool CanAccessProfesor(this ClaimsPrincipal principal, int profesorId)
    {
        if (principal.IsAdmin())
            return true;

        if (principal.IsProfesor())
        {
            var userProfesorId = principal.GetProfesorId();
            return userProfesorId == profesorId;
        }

        return false;
    }

    /// <summary>
    /// Verifica si el usuario puede acceder a datos de un estudiante específico
    /// </summary>
    public static bool CanAccessEstudiante(this ClaimsPrincipal principal, int estudianteId)
    {
        if (principal.IsAdmin() || principal.IsProfesor())
            return true;

        if (principal.IsEstudiante())
        {
            var userEstudianteId = principal.GetEstudianteId();
            return userEstudianteId == estudianteId;
        }

        return false;
    }

    /// <summary>
    /// Verifica si el usuario puede gestionar notas (crear, editar, eliminar)
    /// </summary>
    public static bool CanManageNotas(this ClaimsPrincipal principal, int? profesorId = null)
    {
        if (principal.IsAdmin())
            return true;

        if (principal.IsProfesor())
        {
            if (profesorId.HasValue)
            {
                var userProfesorId = principal.GetProfesorId();
                return userProfesorId == profesorId;
            }
            return true; // Puede gestionar sus propias notas
        }

        return false; // Estudiantes no pueden gestionar notas
    }

    /// <summary>
    /// Verifica si el usuario puede ver notas específicas
    /// </summary>
    public static bool CanViewNota(this ClaimsPrincipal principal, int profesorId, int estudianteId)
    {
        if (principal.IsAdmin())
            return true;

        if (principal.IsProfesor())
        {
            var userProfesorId = principal.GetProfesorId();
            return userProfesorId == profesorId;
        }

        if (principal.IsEstudiante())
        {
            var userEstudianteId = principal.GetEstudianteId();
            return userEstudianteId == estudianteId;
        }

        return false;
    }

    /// <summary>
    /// Verifica si el usuario puede crear una nota para un profesor específico
    /// </summary>
    public static bool CanCreateNota(this ClaimsPrincipal principal, int profesorId)
    {
        if (principal.IsAdmin())
            return true;

        if (principal.IsProfesor())
        {
            var userProfesorId = principal.GetProfesorId();
            return userProfesorId == profesorId;
        }

        return false; // Estudiantes no pueden crear notas
    }
}
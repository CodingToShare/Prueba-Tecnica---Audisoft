namespace AudiSoft.School.Application.DTOs;

/// <summary>
/// DTO para respuesta de login exitoso
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// Token de acceso JWT
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de token (siempre "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Tiempo de expiración del token en segundos
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Fecha y hora de expiración del token
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Información básica del usuario autenticado
    /// </summary>
    public UserInfoDto User { get; set; } = new();
}

/// <summary>
/// Información básica del usuario para respuesta de login
/// </summary>
public class UserInfoDto
{
    /// <summary>
    /// ID del usuario
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre de usuario
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Email del usuario
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Lista de roles del usuario
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// ID del profesor si el usuario está asociado a uno
    /// </summary>
    public int? IdProfesor { get; set; }

    /// <summary>
    /// ID del estudiante si el usuario está asociado a uno
    /// </summary>
    public int? IdEstudiante { get; set; }

    /// <summary>
    /// Nombre del profesor o estudiante asociado
    /// </summary>
    public string? NombreCompleto { get; set; }

    /// <summary>
    /// Último login del usuario
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}
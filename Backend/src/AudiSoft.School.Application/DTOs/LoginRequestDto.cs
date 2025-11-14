namespace AudiSoft.School.Application.DTOs;

/// <summary>
/// DTO para solicitud de login
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Nombre de usuario o email
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Si debe recordar la sesión (token de larga duración)
    /// </summary>
    public bool RememberMe { get; set; } = false;
}
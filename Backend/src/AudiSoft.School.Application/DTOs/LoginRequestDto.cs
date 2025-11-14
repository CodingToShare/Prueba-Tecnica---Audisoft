using System.ComponentModel.DataAnnotations;

namespace AudiSoft.School.Application.DTOs;

/// <summary>
/// DTO para solicitud de login
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Nombre de usuario o email
    /// </summary>
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 100 caracteres")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Si debe recordar la sesión (token de larga duración)
    /// </summary>
    public bool RememberMe { get; set; } = false;
}
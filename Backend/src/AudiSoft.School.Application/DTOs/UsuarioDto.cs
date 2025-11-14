using System.ComponentModel.DataAnnotations;

namespace AudiSoft.School.Application.DTOs;

/// <summary>
/// DTO para transferencia de datos de Usuario.
/// </summary>
public class UsuarioDto
{
    /// <summary>
    /// Identificador único del usuario
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre de usuario único
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Dirección de correo electrónico del usuario
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Indica si el usuario está activo en el sistema
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Fecha y hora del último login del usuario
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// ID del profesor asociado al usuario (si aplica)
    /// </summary>
    public int? IdProfesor { get; set; }

    /// <summary>
    /// Nombre del profesor asociado al usuario
    /// </summary>
    public string? NombreProfesor { get; set; }

    /// <summary>
    /// ID del estudiante asociado al usuario (si aplica)
    /// </summary>
    public int? IdEstudiante { get; set; }

    /// <summary>
    /// Nombre del estudiante asociado al usuario
    /// </summary>
    public string? NombreEstudiante { get; set; }

    /// <summary>
    /// Lista de roles asignados al usuario
    /// </summary>
    public List<string> Roles { get; set; } = new List<string>();

    /// <summary>
    /// Fecha de creación del usuario
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para crear un nuevo Usuario.
/// </summary>
public class CreateUsuarioDto
{
    /// <summary>
    /// Nombre de usuario único. Solo letras, números, puntos, guiones y guiones bajos.
    /// </summary>
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 100 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números, puntos, guiones y guiones bajos")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario. Debe contener mayúsculas, minúsculas, números y caracteres especiales.
    /// </summary>
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", ErrorMessage = "La contraseña debe contener al menos una mayúscula, una minúscula, un número y un carácter especial")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Dirección de correo electrónico del usuario (opcional)
    /// </summary>
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(255, ErrorMessage = "El email no puede tener más de 255 caracteres")]
    public string? Email { get; set; }

    /// <summary>
    /// ID del profesor a asociar con este usuario (opcional)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "El ID del profesor debe ser un número positivo")]
    public int? IdProfesor { get; set; }

    /// <summary>
    /// ID del estudiante a asociar con este usuario (opcional)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "El ID del estudiante debe ser un número positivo")]
    public int? IdEstudiante { get; set; }

    /// <summary>
    /// Indica si el usuario debe estar activo al crearse
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Lista de IDs de roles a asignar al usuario
    /// </summary>
    public List<int> RoleIds { get; set; } = new List<int>();
}

/// <summary>
/// DTO para actualizar un Usuario existente.
/// </summary>
public class UpdateUsuarioDto
{
    /// <summary>
    /// Nueva dirección de correo electrónico del usuario
    /// </summary>
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(255, ErrorMessage = "El email no puede tener más de 255 caracteres")]
    public string? Email { get; set; }

    /// <summary>
    /// Nuevo ID del profesor a asociar con este usuario
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "El ID del profesor debe ser un número positivo")]
    public int? IdProfesor { get; set; }

    /// <summary>
    /// Nuevo ID del estudiante a asociar con este usuario
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "El ID del estudiante debe ser un número positivo")]
    public int? IdEstudiante { get; set; }

    /// <summary>
    /// Nuevo estado activo del usuario
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO para cambio de contraseña.
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// Contraseña actual del usuario para verificación
    /// </summary>
    [Required(ErrorMessage = "La contraseña actual es obligatoria")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Nueva contraseña. Debe contener mayúsculas, minúsculas, números y caracteres especiales.
    /// </summary>
    [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La nueva contraseña debe tener entre 6 y 100 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", ErrorMessage = "La nueva contraseña debe contener al menos una mayúscula, una minúscula, un número y un carácter especial")]
    public string NewPassword { get; set; } = string.Empty;
}
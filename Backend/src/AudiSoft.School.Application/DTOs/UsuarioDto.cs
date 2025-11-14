namespace AudiSoft.School.Application.DTOs;

/// <summary>
/// DTO para transferencia de datos de Usuario.
/// </summary>
public class UsuarioDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int? IdProfesor { get; set; }
    public string? NombreProfesor { get; set; }
    public int? IdEstudiante { get; set; }
    public string? NombreEstudiante { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para crear un nuevo Usuario.
/// </summary>
public class CreateUsuarioDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? IdProfesor { get; set; }
    public int? IdEstudiante { get; set; }
    public bool IsActive { get; set; } = true;
    public List<int> RoleIds { get; set; } = new List<int>();
}

/// <summary>
/// DTO para actualizar un Usuario existente.
/// </summary>
public class UpdateUsuarioDto
{
    public string? Email { get; set; }
    public int? IdProfesor { get; set; }
    public int? IdEstudiante { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO para cambio de contraseña.
/// </summary>
public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
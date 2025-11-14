namespace AudiSoft.School.Application.DTOs;

/// <summary>
/// DTO para transferencia de datos de Rol.
/// </summary>
public class RolDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool IsActive { get; set; }
    public int UsuarioCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para crear un nuevo Rol.
/// </summary>
public class CreateRolDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO para actualizar un Rol existente.
/// </summary>
public class UpdateRolDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool IsActive { get; set; }
}
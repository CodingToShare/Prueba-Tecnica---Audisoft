using System.ComponentModel.DataAnnotations;

namespace AudiSoft.School.Application.DTOs;

public class UpdateEstudianteDto
{
    [Required(ErrorMessage = "El nombre del estudiante es obligatorio")]
    [StringLength(255, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 255 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
    public string Nombre { get; set; } = null!;
}

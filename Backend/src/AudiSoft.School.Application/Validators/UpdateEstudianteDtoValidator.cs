using AudiSoft.School.Application.DTOs;
using FluentValidation;

namespace AudiSoft.School.Application.Validators;

public class UpdateEstudianteDtoValidator : AbstractValidator<UpdateEstudianteDto>
{
    public UpdateEstudianteDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
                .WithMessage("El nombre del estudiante es requerido")
            .MinimumLength(3)
                .WithMessage("El nombre debe tener al menos 3 caracteres")
            .MaximumLength(255)
                .WithMessage("El nombre no puede exceder 255 caracteres")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\-0-9]+$")
                .WithMessage("El nombre solo puede contener letras, espacios, guiones y números");
    }
}

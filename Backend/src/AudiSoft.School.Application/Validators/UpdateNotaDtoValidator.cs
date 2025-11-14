using AudiSoft.School.Application.DTOs;
using FluentValidation;

namespace AudiSoft.School.Application.Validators;

public class UpdateNotaDtoValidator : AbstractValidator<UpdateNotaDto>
{
    public UpdateNotaDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre de la nota es requerido")
            .MaximumLength(255)
            .WithMessage("El nombre no puede exceder 255 caracteres");

        RuleFor(x => x.Valor)
            .InclusiveBetween(0, 100)
            .WithMessage("El valor de la nota debe estar entre 0 y 100");

        RuleFor(x => x.IdProfesor)
            .GreaterThan(0)
            .WithMessage("El ID del profesor debe ser válido");

        RuleFor(x => x.IdEstudiante)
            .GreaterThan(0)
            .WithMessage("El ID del estudiante debe ser válido");
    }
}

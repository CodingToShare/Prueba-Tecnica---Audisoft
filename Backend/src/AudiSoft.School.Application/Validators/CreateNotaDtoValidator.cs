using AudiSoft.School.Application.DTOs;
using FluentValidation;

namespace AudiSoft.School.Application.Validators;

/// <summary>
/// Validador para CreateNotaDto.
/// </summary>
public class CreateNotaDtoValidator : AbstractValidator<CreateNotaDto>
{
    public CreateNotaDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
                .WithMessage("El nombre de la nota es requerido")
            .NotNull()
                .WithMessage("El nombre no puede ser nulo")
            .MaximumLength(255)
                .WithMessage("El nombre no puede exceder 255 caracteres");

        RuleFor(x => x.Valor)
            .InclusiveBetween(0, 100)
                .WithMessage("El valor de la nota debe estar entre 0 y 100")
            .PrecisionScale(5, 2, true)
                .WithMessage("El valor debe tener máximo 3 dígitos enteros y 2 decimales");

        RuleFor(x => x.IdProfesor)
            .GreaterThan(0)
                .WithMessage("El ID del profesor debe ser válido");

        RuleFor(x => x.IdEstudiante)
            .GreaterThan(0)
                .WithMessage("El ID del estudiante debe ser válido");
    }
}

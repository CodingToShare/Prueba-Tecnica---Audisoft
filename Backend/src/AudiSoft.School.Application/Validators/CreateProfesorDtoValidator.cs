using AudiSoft.School.Application.DTOs;
using FluentValidation;

namespace AudiSoft.School.Application.Validators;

/// <summary>
/// Validador para CreateProfesorDto.
/// </summary>
public class CreateProfesorDtoValidator : AbstractValidator<CreateProfesorDto>
{
    public CreateProfesorDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
                .WithMessage("El nombre del profesor es requerido")
            .NotNull()
                .WithMessage("El nombre del profesor no puede ser nulo")
            .MinimumLength(3)
                .WithMessage("El nombre debe tener al menos 3 caracteres")
            .MaximumLength(255)
                .WithMessage("El nombre no puede exceder 255 caracteres")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")
                .WithMessage("El nombre solo puede contener letras y espacios");
    }
}

using AudiSoft.School.Application.DTOs;
using FluentValidation;

namespace AudiSoft.School.Application.Validators;

public class UpdateProfesorDtoValidator : AbstractValidator<UpdateProfesorDto>
{
    public UpdateProfesorDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
                .WithMessage("El nombre del profesor es requerido")
            .MinimumLength(3)
                .WithMessage("El nombre debe tener al menos 3 caracteres")
            .MaximumLength(255)
                .WithMessage("El nombre no puede exceder 255 caracteres")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")
                .WithMessage("El nombre solo puede contener letras y espacios");
    }
}

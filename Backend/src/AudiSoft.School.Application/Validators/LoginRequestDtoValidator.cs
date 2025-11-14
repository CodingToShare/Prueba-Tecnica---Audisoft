using AudiSoft.School.Application.DTOs;
using FluentValidation;

namespace AudiSoft.School.Application.Validators;

/// <summary>
/// Validador para LoginRequestDto
/// </summary>
public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    /// <summary>
    /// Constructor que define las reglas de validación
    /// </summary>
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("El nombre de usuario es requerido")
            .Length(3, 100)
            .WithMessage("El nombre de usuario debe tener entre 3 y 100 caracteres")
            .Matches(@"^[a-zA-Z0-9._@-]+$")
            .WithMessage("El nombre de usuario solo puede contener letras, números, puntos, guiones y @");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("La contraseña es requerida")
            .MinimumLength(6)
            .WithMessage("La contraseña debe tener al menos 6 caracteres")
            .MaximumLength(100)
            .WithMessage("La contraseña no puede tener más de 100 caracteres");
    }
}
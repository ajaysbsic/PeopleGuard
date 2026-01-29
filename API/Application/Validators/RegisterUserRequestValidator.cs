using EmployeeInvestigationSystem.Application.DTOs.Auth;
using FluentValidation;

namespace EmployeeInvestigationSystem.Application.Validators;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequestDto>
{
    private static readonly string[] AllowedRoles = [
        "Admin", "Business", "ER", "HR", "ITAdmin", "Management"
    ];

    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[A-Za-z]").WithMessage("Password must contain a letter.")
            .Matches("^[A-Za-z0-9]*$").WithMessage("Password must be alphanumeric.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => AllowedRoles.Contains(role))
            .WithMessage("Role must be one of: Admin, Business, ER, HR, ITAdmin, Management.");
    }
}

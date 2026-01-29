using EmployeeInvestigationSystem.Application.DTOs.Auth;
using FluentValidation;

namespace EmployeeInvestigationSystem.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

using EmployeeInvestigationSystem.Application.DTOs;
using FluentValidation;

namespace EmployeeInvestigationSystem.Application.Validators;

public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty().Length(1, 50);
        RuleFor(x => x.Name).NotEmpty().Length(2, 200);
        RuleFor(x => x.Department).NotEmpty().Length(1, 100);
        RuleFor(x => x.Factory).NotEmpty().Length(1, 100);
        RuleFor(x => x.Designation).NotEmpty().Length(1, 100);
    }
}

public class UpdateEmployeeDtoValidator : AbstractValidator<UpdateEmployeeDto>
{
    public UpdateEmployeeDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 200);
        RuleFor(x => x.Department).NotEmpty().Length(1, 100);
        RuleFor(x => x.Factory).NotEmpty().Length(1, 100);
        RuleFor(x => x.Designation).NotEmpty().Length(1, 100);
    }
}

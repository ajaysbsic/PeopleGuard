using EmployeeInvestigationSystem.Application.DTOs;
using FluentValidation;

namespace EmployeeInvestigationSystem.Application.Validators;

public class CreateInvestigationDtoValidator : AbstractValidator<CreateInvestigationDto>
{
    public CreateInvestigationDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().Length(5, 200);
        RuleFor(x => x.Description).NotEmpty().Length(10, 2000);
    }
}

public class UpdateInvestigationDtoValidator : AbstractValidator<UpdateInvestigationDto>
{
    public UpdateInvestigationDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(5, 200);
        RuleFor(x => x.Description).NotEmpty().Length(10, 2000);
    }
}

public class CreateInvestigationRemarkDtoValidator : AbstractValidator<CreateInvestigationRemarkDto>
{
    public CreateInvestigationRemarkDtoValidator()
    {
        RuleFor(x => x.InvestigationId).NotEmpty();
        RuleFor(x => x.Remark).NotEmpty().Length(5, 1000);
    }
}

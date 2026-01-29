using EmployeeInvestigationSystem.Domain.Entities;

namespace EmployeeInvestigationSystem.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string Generate(ApplicationUser user, IList<string> roles, DateTime expiresAtUtc);
}

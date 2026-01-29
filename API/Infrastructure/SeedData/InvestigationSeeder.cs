using EmployeeInvestigationSystem.Domain.Entities;
using EmployeeInvestigationSystem.Domain.Enums;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmployeeInvestigationSystem.Infrastructure.SeedData;

public class InvestigationSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<InvestigationSeeder> _logger;

    public InvestigationSeeder(AppDbContext context, ILogger<InvestigationSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _context.Set<Investigation>().AnyAsync())
        {
            _logger.LogInformation("Investigations already seeded, skipping.");
            return;
        }

        var employees = await _context.Set<Employee>().Take(10).ToListAsync();
        if (!employees.Any())
        {
            _logger.LogWarning("No employees found. Cannot seed investigations.");
            return;
        }

        var investigations = new List<Investigation>
        {
            new Investigation
            {
                Id = Guid.NewGuid(),
                EmployeeId = employees[0].Id,
                Title = "Safety Protocol Violation - PPE",
                Description = "Employee was observed not wearing required PPE in manufacturing area.",
                CaseType = InvestigationCaseType.Safety,
                Status = InvestigationStatus.Open,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Investigation
            {
                Id = Guid.NewGuid(),
                EmployeeId = employees[1].Id,
                Title = "Attendance Issue Investigation",
                Description = "Multiple instances of unauthorized absence reported by supervisor.",
                CaseType = InvestigationCaseType.Violation,
                Status = InvestigationStatus.UnderInvestigation,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Investigation
            {
                Id = Guid.NewGuid(),
                EmployeeId = employees[2].Id,
                Title = "Workplace Misconduct Report",
                Description = "Reported incident of inappropriate behavior during work hours.",
                CaseType = InvestigationCaseType.Misbehavior,
                Status = InvestigationStatus.Open,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Investigation
            {
                Id = Guid.NewGuid(),
                EmployeeId = employees[3].Id,
                Title = "Quality Control Failure",
                Description = "Investigation into product defects traced to operator error.",
                CaseType = InvestigationCaseType.Investigation,
                Status = InvestigationStatus.Closed,
                Outcome = WarningOutcome.VerbalWarning,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                ClosedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Investigation
            {
                Id = Guid.NewGuid(),
                EmployeeId = employees[4].Id,
                Title = "Equipment Misuse Report",
                Description = "Employee used company equipment for unauthorized purposes.",
                CaseType = InvestigationCaseType.Violation,
                Status = InvestigationStatus.UnderInvestigation,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Investigation
            {
                Id = Guid.NewGuid(),
                EmployeeId = employees[5].Id,
                Title = "Safety Incident - Minor Injury",
                Description = "Investigation following a minor workplace injury incident.",
                CaseType = InvestigationCaseType.Safety,
                Status = InvestigationStatus.Open,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Investigation
            {
                Id = Guid.NewGuid(),
                EmployeeId = employees[6].Id,
                Title = "Harassment Complaint Investigation",
                Description = "Investigation into harassment complaint from coworker.",
                CaseType = InvestigationCaseType.Misbehavior,
                Status = InvestigationStatus.UnderInvestigation,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Investigation
            {
                Id = Guid.NewGuid(),
                EmployeeId = employees[7].Id,
                Title = "Time Sheet Fraud Investigation",
                Description = "Discrepancies found in time records submitted by employee.",
                CaseType = InvestigationCaseType.Violation,
                Status = InvestigationStatus.Closed,
                Outcome = WarningOutcome.WrittenWarning,
                CreatedAt = DateTime.UtcNow.AddDays(-45),
                ClosedAt = DateTime.UtcNow.AddDays(-25)
            }
        };

        await _context.Set<Investigation>().AddRangeAsync(investigations);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} investigations", investigations.Count);
    }
}

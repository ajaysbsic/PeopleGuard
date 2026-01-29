using AutoMapper;
using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Application.Interfaces;
using EmployeeInvestigationSystem.Domain.Enums;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace EmployeeInvestigationSystem.Infrastructure.Services;

/// <summary>
/// Service for employee history and statistics.
/// </summary>
public class EmployeeHistoryService : IEmployeeHistoryService
{
    private readonly AppDbContext _context;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;

    public EmployeeHistoryService(
        AppDbContext context,
        IEmployeeRepository employeeRepository,
        IMapper mapper)
    {
        _context = context;
        _employeeRepository = employeeRepository;
        _mapper = mapper;
    }

    public async Task<EmployeeHistoryDto> GetEmployeeHistoryAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");
        }

        var investigations = await _context.Investigations
            .Where(i => i.EmployeeId == employeeId && !i.IsDeleted)
            .Include(i => i.Remarks)
            .Include(i => i.Attachments)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);

        var warnings = await _context.WarningLetters
            .Where(w => w.EmployeeId == employeeId)
            .OrderByDescending(w => w.IssuedAt)
            .ToListAsync(cancellationToken);

        var investigationHistory = investigations.Select(i => new InvestigationHistoryItemDto
        {
            InvestigationId = i.Id,
            Title = i.Title,
            Description = i.Description,
            CaseType = i.CaseType.ToString(),
            Status = i.Status.ToString(),
            Outcome = i.Outcome?.ToString(),
            CreatedAt = i.CreatedAt,
            ClosedAt = i.ClosedAt,
            RemarksCount = i.Remarks.Count,
            AttachmentsCount = i.Attachments.Count
        });

        var warningHistory = warnings.Select(w => new WarningHistoryItemDto
        {
            WarningLetterId = w.Id,
            InvestigationId = w.InvestigationId,
            Outcome = w.Outcome.ToString(),
            IssuedAt = w.IssuedAt,
            Reason = w.LetterContent
        });

        var verbalWarnings = warnings.Count(w => w.Outcome == WarningOutcome.VerbalWarning);
        var writtenWarnings = warnings.Count(w => w.Outcome == WarningOutcome.WrittenWarning);
        var noActionCases = investigations.Count(i => i.Outcome == WarningOutcome.NoAction);

        return new EmployeeHistoryDto
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            EmployeeIdNumber = employee.EmployeeId,
            Department = employee.Department,
            Factory = employee.Factory,
            TotalInvestigations = investigations.Count,
            TotalWarnings = warnings.Count,
            VerbalWarnings = verbalWarnings,
            WrittenWarnings = writtenWarnings,
            NoActionCases = noActionCases,
            LastInvestigationDate = investigations.FirstOrDefault()?.CreatedAt,
            LastWarningDate = warnings.FirstOrDefault()?.IssuedAt,
            InvestigationHistory = investigationHistory,
            WarningHistory = warningHistory
        };
    }

    public async Task<EmployeeStatisticsDto> GetEmployeeStatisticsAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");
        }

        var investigations = await _context.Investigations
            .Where(i => i.EmployeeId == employeeId && !i.IsDeleted)
            .ToListAsync(cancellationToken);

        var warnings = await _context.WarningLetters
            .Where(w => w.EmployeeId == employeeId)
            .ToListAsync(cancellationToken);

        var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);
        var recentViolations = investigations.Count(i => i.CreatedAt >= ninetyDaysAgo);

        var closedInvestigations = investigations.Where(i => i.Status == InvestigationStatus.Closed).ToList();
        double? averageResolutionDays = null;
        if (closedInvestigations.Any())
        {
            var resolutionDays = closedInvestigations
                .Where(i => i.ClosedAt.HasValue)
                .Select(i => (i.ClosedAt.Value - i.CreatedAt).TotalDays)
                .ToList();
            if (resolutionDays.Any())
            {
                averageResolutionDays = resolutionDays.Average();
            }
        }

        var totalWarnings = warnings.Count;
        var verbalWarnings = warnings.Count(w => w.Outcome == WarningOutcome.VerbalWarning);
        var writtenWarnings = warnings.Count(w => w.Outcome == WarningOutcome.WrittenWarning);

        var riskLevel = CalculateRiskLevel(
            investigations.Count,
            totalWarnings,
            writtenWarnings,
            recentViolations);

        return new EmployeeStatisticsDto
        {
            EmployeeId = employeeId,
            TotalInvestigations = investigations.Count,
            ActiveInvestigations = investigations.Count(i => i.Status != InvestigationStatus.Closed),
            ClosedInvestigations = closedInvestigations.Count,
            TotalWarnings = totalWarnings,
            VerbalWarnings = verbalWarnings,
            WrittenWarnings = writtenWarnings,
            ViolationsLast90Days = recentViolations,
            RiskLevel = riskLevel,
            AverageResolutionDays = averageResolutionDays
        };
    }

    private static string CalculateRiskLevel(int totalInvestigations, int totalWarnings, int writtenWarnings, int recentViolations)
    {
        // Risk scoring: written warnings (weight 3), total warnings (weight 1.5), recent violations (weight 1)
        var riskScore = (writtenWarnings * 3) + (totalWarnings * 1.5) + recentViolations;

        return riskScore switch
        {
            >= 15 => "Critical",
            >= 10 => "High",
            >= 5 => "Medium",
            _ => "Low"
        };
    }
}

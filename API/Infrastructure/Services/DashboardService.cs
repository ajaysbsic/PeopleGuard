using ClosedXML.Excel;
using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Application.Interfaces;
using EmployeeInvestigationSystem.Domain.Enums;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace EmployeeInvestigationSystem.Infrastructure.Services;

/// <summary>
/// Service for dashboard data and analytics.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly IPdfGenerationService _pdfGenerationService;

    public DashboardService(AppDbContext context, IPdfGenerationService pdfGenerationService)
    {
        _context = context;
        _pdfGenerationService = pdfGenerationService;
    }

    public async Task<DashboardDto> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        var totalViolations = await _context.Investigations.CountAsync(i => !i.IsDeleted);
        var activeInvestigations = await _context.Investigations
            .CountAsync(i => !i.IsDeleted && i.Status != InvestigationStatus.Closed);
        var pendingWarnings = await _context.WarningLetters.CountAsync();
        var totalEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted);

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var employeesWithViolations = await _context.Investigations
            .Where(i => !i.IsDeleted && i.CreatedAt >= thirtyDaysAgo)
            .Select(i => i.EmployeeId)
            .Distinct()
            .CountAsync();

        var violationsByFactory = await GetViolationsByFactoryAsync(cancellationToken);
        var violationsByDepartment = await GetViolationsByDepartmentAsync(cancellationToken);
        var violationsByType = await GetViolationsByTypeAsync(cancellationToken);
        var violationsByOutcome = await GetViolationsByOutcomeAsync(cancellationToken);
        var trends = await GetViolationsTrendAsync(cancellationToken);
        var topViolators = await GetTopViolatorsAsync(5, cancellationToken);
        var recentInvestigations = await GetRecentInvestigationsAsync(10, cancellationToken);

        return new DashboardDto
        {
            TotalViolations = totalViolations,
            ActiveInvestigations = activeInvestigations,
            PendingWarnings = pendingWarnings,
            TotalEmployees = totalEmployees,
            EmployeesWithViolations = employeesWithViolations,
            ViolationsByFactory = violationsByFactory,
            ViolationsByDepartment = violationsByDepartment,
            ViolationsByType = violationsByType,
            ViolationsByOutcome = violationsByOutcome,
            ViolationsTrend = trends,
            TopViolators = topViolators,
            RecentInvestigations = recentInvestigations
        };
    }

    public async Task<IEnumerable<ChartDataDto>> GetViolationsByFactoryAsync(CancellationToken cancellationToken = default)
    {
        var data = await _context.Investigations
            .Where(i => !i.IsDeleted)
            .Include(i => i.Employee)
            .GroupBy(i => i.Employee!.Factory)
            .Select(g => new { Factory = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var total = data.Sum(x => x.Count);
        return data.Select(x => new ChartDataDto
        {
            Label = x.Factory,
            Value = x.Count,
            Percentage = total > 0 ? Math.Round((decimal)x.Count / total * 100, 2) : 0
        });
    }

    public async Task<IEnumerable<ChartDataDto>> GetViolationsByDepartmentAsync(CancellationToken cancellationToken = default)
    {
        var data = await _context.Investigations
            .Where(i => !i.IsDeleted)
            .Include(i => i.Employee)
            .GroupBy(i => i.Employee!.Department)
            .Select(g => new { Department = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var total = data.Sum(x => x.Count);
        return data.Select(x => new ChartDataDto
        {
            Label = x.Department,
            Value = x.Count,
            Percentage = total > 0 ? Math.Round((decimal)x.Count / total * 100, 2) : 0
        });
    }

    public async Task<IEnumerable<ChartDataDto>> GetViolationsByTypeAsync(CancellationToken cancellationToken = default)
    {
        var data = await _context.Investigations
            .Where(i => !i.IsDeleted)
            .GroupBy(i => i.CaseType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var total = data.Sum(x => x.Count);
        return data.Select(x => new ChartDataDto
        {
            Label = x.Type.ToString(),
            Value = x.Count,
            Percentage = total > 0 ? Math.Round((decimal)x.Count / total * 100, 2) : 0
        });
    }

    public async Task<IEnumerable<ChartDataDto>> GetViolationsByOutcomeAsync(CancellationToken cancellationToken = default)
    {
        var data = await _context.Investigations
            .Where(i => !i.IsDeleted && i.Outcome.HasValue)
            .GroupBy(i => i.Outcome)
            .Select(g => new { Outcome = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var total = data.Sum(x => x.Count);
        return data.Select(x => new ChartDataDto
        {
            Label = x.Outcome.HasValue ? x.Outcome.Value.ToString() : "Unknown",
            Value = x.Count,
            Percentage = total > 0 ? Math.Round((decimal)x.Count / total * 100, 2) : 0
        });
    }

    public async Task<IEnumerable<TrendDataDto>> GetViolationsTrendAsync(CancellationToken cancellationToken = default)
    {
        var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-12);
        var data = await _context.Investigations
            .Where(i => !i.IsDeleted && i.CreatedAt >= twelveMonthsAgo)
            .GroupBy(i => new { i.CreatedAt.Year, i.CreatedAt.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count()
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        return data.Select(x => new TrendDataDto
        {
            Month = $"{x.Month:D2}/{x.Year}",
            Count = x.Count
        });
    }

    public async Task<IEnumerable<TopViolatorDto>> GetTopViolatorsAsync(int topCount = 10, CancellationToken cancellationToken = default)
    {
        var data = await _context.Investigations
            .Where(i => !i.IsDeleted)
            .Include(i => i.Employee)
            .GroupBy(i => i.EmployeeId)
            .Select(g => new
            {
                EmployeeId = g.Key,
                ViolationCount = g.Count(),
                Employee = g.First().Employee
            })
            .OrderByDescending(x => x.ViolationCount)
            .Take(topCount)
            .ToListAsync(cancellationToken);

        var result = new List<TopViolatorDto>();
        foreach (var item in data)
        {
            var warnings = await _context.WarningLetters
                .CountAsync(w => w.EmployeeId == item.EmployeeId);
            var writtenWarnings = await _context.WarningLetters
                .CountAsync(w => w.EmployeeId == item.EmployeeId && w.Outcome == WarningOutcome.WrittenWarning);

            var riskScore = (writtenWarnings * 3) + (warnings * 1.5) + item.ViolationCount;
            var riskLevel = riskScore >= 15 ? "Critical" : riskScore >= 10 ? "High" : riskScore >= 5 ? "Medium" : "Low";

            result.Add(new TopViolatorDto
            {
                EmployeeId = item.EmployeeId,
                Name = item.Employee!.Name,
                EmployeeIdNumber = item.Employee.EmployeeId,
                Department = item.Employee.Department,
                Factory = item.Employee.Factory,
                ViolationCount = item.ViolationCount,
                RiskLevel = riskLevel
            });
        }

        return result;
    }

    public async Task<IEnumerable<RecentInvestigationDto>> GetRecentInvestigationsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Investigations
            .Where(i => !i.IsDeleted)
            .Include(i => i.Employee)
            .OrderByDescending(i => i.CreatedAt)
            .Take(count)
            .Select(i => new RecentInvestigationDto
            {
                InvestigationId = i.Id,
                Title = i.Title,
                EmployeeName = i.Employee!.Name,
                CaseType = i.CaseType.ToString(),
                Status = i.Status.ToString(),
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<byte[]> ExportToExcelAsync(ExportReportRequest request, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetDashboardDataAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        
        // Summary Sheet
        var summarySheet = workbook.Worksheets.Add("Summary");
        summarySheet.Cell("A1").Value = "Investigation & Violation Report";
        summarySheet.Cell("A1").Style.Font.Bold = true;
        summarySheet.Cell("A3").Value = "Metric";
        summarySheet.Cell("B3").Value = "Value";
        summarySheet.Cell("A3:B3").Style.Font.Bold = true;

        var row = 4;
        summarySheet.Cell($"A{row}").Value = "Total Violations";
        summarySheet.Cell($"B{row}").Value = dashboard.TotalViolations;
        row++;
        summarySheet.Cell($"A{row}").Value = "Active Investigations";
        summarySheet.Cell($"B{row}").Value = dashboard.ActiveInvestigations;
        row++;
        summarySheet.Cell($"A{row}").Value = "Pending Warnings";
        summarySheet.Cell($"B{row}").Value = dashboard.PendingWarnings;
        row++;
        summarySheet.Cell($"A{row}").Value = "Total Employees";
        summarySheet.Cell($"B{row}").Value = dashboard.TotalEmployees;
        row++;
        summarySheet.Cell($"A{row}").Value = "Employees with Violations (30d)";
        summarySheet.Cell($"B{row}").Value = dashboard.EmployeesWithViolations;

        // Violations by Factory
        var factorySheet = workbook.Worksheets.Add("Violations by Factory");
        factorySheet.Cell("A1").Value = "Factory";
        factorySheet.Cell("B1").Value = "Count";
        factorySheet.Cell("C1").Value = "Percentage";
        factorySheet.Cell("A1:C1").Style.Font.Bold = true;
        row = 2;
        foreach (var item in dashboard.ViolationsByFactory)
        {
            factorySheet.Cell($"A{row}").Value = item.Label;
            factorySheet.Cell($"B{row}").Value = item.Value;
            factorySheet.Cell($"C{row}").Value = $"{item.Percentage}%";
            row++;
        }

        // Top Violators
        var violatorsSheet = workbook.Worksheets.Add("Top Violators");
        violatorsSheet.Cell("A1").Value = "Name";
        violatorsSheet.Cell("B1").Value = "Employee ID";
        violatorsSheet.Cell("C1").Value = "Department";
        violatorsSheet.Cell("D1").Value = "Factory";
        violatorsSheet.Cell("E1").Value = "Violations";
        violatorsSheet.Cell("F1").Value = "Risk Level";
        violatorsSheet.Cell("A1:F1").Style.Font.Bold = true;
        row = 2;
        foreach (var violator in dashboard.TopViolators)
        {
            violatorsSheet.Cell($"A{row}").Value = violator.Name;
            violatorsSheet.Cell($"B{row}").Value = violator.EmployeeIdNumber;
            violatorsSheet.Cell($"C{row}").Value = violator.Department;
            violatorsSheet.Cell($"D{row}").Value = violator.Factory;
            violatorsSheet.Cell($"E{row}").Value = violator.ViolationCount;
            violatorsSheet.Cell($"F{row}").Value = violator.RiskLevel;
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportToPdfAsync(ExportReportRequest request, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetDashboardDataAsync(cancellationToken);

        var summaryText = $@"
Investigation & Violation Report
Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}

SUMMARY STATISTICS
==================
Total Violations: {dashboard.TotalViolations}
Active Investigations: {dashboard.ActiveInvestigations}
Pending Warnings: {dashboard.PendingWarnings}
Total Employees: {dashboard.TotalEmployees}
Employees with Violations (30 days): {dashboard.EmployeesWithViolations}

TOP VIOLATORS
=============";

        foreach (var violator in dashboard.TopViolators.Take(10))
        {
            summaryText += $@"
- {violator.Name} ({violator.EmployeeIdNumber})
  Department: {violator.Department}, Factory: {violator.Factory}
  Violations: {violator.ViolationCount}, Risk Level: {violator.RiskLevel}";
        }

        // For now, return empty bytes (PDF generation would require itext7 advanced usage)
        // In production, use itext7 to format this as a proper PDF
        return System.Text.Encoding.UTF8.GetBytes(summaryText);
    }
}

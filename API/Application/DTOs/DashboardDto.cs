namespace EmployeeInvestigationSystem.Application.DTOs;

/// <summary>
/// Dashboard overview data with charts and statistics.
/// </summary>
public class DashboardDto
{
    /// <summary>
    /// Total violations count.
    /// </summary>
    public int TotalViolations { get; set; }

    /// <summary>
    /// Active investigations count.
    /// </summary>
    public int ActiveInvestigations { get; set; }

    /// <summary>
    /// Pending warnings count.
    /// </summary>
    public int PendingWarnings { get; set; }

    /// <summary>
    /// Total employees.
    /// </summary>
    public int TotalEmployees { get; set; }

    /// <summary>
    /// Employees with violations in last 30 days.
    /// </summary>
    public int EmployeesWithViolations { get; set; }

    /// <summary>
    /// Violations by factory chart data.
    /// </summary>
    public IEnumerable<ChartDataDto> ViolationsByFactory { get; set; } = new List<ChartDataDto>();

    /// <summary>
    /// Violations by department chart data.
    /// </summary>
    public IEnumerable<ChartDataDto> ViolationsByDepartment { get; set; } = new List<ChartDataDto>();

    /// <summary>
    /// Violations by type chart data.
    /// </summary>
    public IEnumerable<ChartDataDto> ViolationsByType { get; set; } = new List<ChartDataDto>();

    /// <summary>
    /// Violations by outcome chart data.
    /// </summary>
    public IEnumerable<ChartDataDto> ViolationsByOutcome { get; set; } = new List<ChartDataDto>();

    /// <summary>
    /// Trend data for last 12 months.
    /// </summary>
    public IEnumerable<TrendDataDto> ViolationsTrend { get; set; } = new List<TrendDataDto>();

    /// <summary>
    /// Top violators list.
    /// </summary>
    public IEnumerable<TopViolatorDto> TopViolators { get; set; } = new List<TopViolatorDto>();

    /// <summary>
    /// Recent investigations.
    /// </summary>
    public IEnumerable<RecentInvestigationDto> RecentInvestigations { get; set; } = new List<RecentInvestigationDto>();
}

/// <summary>
/// Chart data point.
/// </summary>
public class ChartDataDto
{
    /// <summary>
    /// Category label.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Count value.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Percentage of total.
    /// </summary>
    public decimal Percentage { get; set; }
}

/// <summary>
/// Trend data for time-series charts.
/// </summary>
public class TrendDataDto
{
    /// <summary>
    /// Month/Year label.
    /// </summary>
    public string Month { get; set; } = string.Empty;

    /// <summary>
    /// Count for the month.
    /// </summary>
    public int Count { get; set; }
}

/// <summary>
/// Top violator in the system.
/// </summary>
public class TopViolatorDto
{
    /// <summary>
    /// Employee ID.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Employee name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Employee ID number.
    /// </summary>
    public string EmployeeIdNumber { get; set; } = string.Empty;

    /// <summary>
    /// Department.
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Factory.
    /// </summary>
    public string Factory { get; set; } = string.Empty;

    /// <summary>
    /// Number of violations.
    /// </summary>
    public int ViolationCount { get; set; }

    /// <summary>
    /// Risk level.
    /// </summary>
    public string RiskLevel { get; set; } = "Low";
}

/// <summary>
/// Recent investigation summary.
/// </summary>
public class RecentInvestigationDto
{
    /// <summary>
    /// Investigation ID.
    /// </summary>
    public Guid InvestigationId { get; set; }

    /// <summary>
    /// Investigation title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Employee name.
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Case type.
    /// </summary>
    public string CaseType { get; set; } = string.Empty;

    /// <summary>
    /// Status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Created date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Export report request.
/// </summary>
public class ExportReportRequest
{
    /// <summary>
    /// Report type: Excel or PDF.
    /// </summary>
    public string ReportType { get; set; } = "Excel";

    /// <summary>
    /// Start date for report range.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for report range.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filter by factory (optional).
    /// </summary>
    public string? FactoryFilter { get; set; }

    /// <summary>
    /// Filter by department (optional).
    /// </summary>
    public string? DepartmentFilter { get; set; }
}

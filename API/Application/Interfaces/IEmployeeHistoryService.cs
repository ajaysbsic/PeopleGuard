using EmployeeInvestigationSystem.Application.DTOs;

namespace EmployeeInvestigationSystem.Application.Interfaces;

/// <summary>
/// Service for employee history and violation records.
/// </summary>
public interface IEmployeeHistoryService
{
    /// <summary>
    /// Get complete history for an employee including all investigations and warnings.
    /// </summary>
    /// <param name="employeeId">ID of the employee</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Employee history DTO with investigations and warnings</returns>
    Task<EmployeeHistoryDto> GetEmployeeHistoryAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get employee statistics (violation counts, warning counts, etc.).
    /// </summary>
    /// <param name="employeeId">ID of the employee</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Employee statistics DTO</returns>
    Task<EmployeeStatisticsDto> GetEmployeeStatisticsAsync(Guid employeeId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Employee statistics DTO.
/// </summary>
public class EmployeeStatisticsDto
{
    /// <summary>
    /// Employee ID.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Total investigations.
    /// </summary>
    public int TotalInvestigations { get; set; }

    /// <summary>
    /// Active investigations (not closed).
    /// </summary>
    public int ActiveInvestigations { get; set; }

    /// <summary>
    /// Closed investigations.
    /// </summary>
    public int ClosedInvestigations { get; set; }

    /// <summary>
    /// Total warnings issued.
    /// </summary>
    public int TotalWarnings { get; set; }

    /// <summary>
    /// Verbal warnings.
    /// </summary>
    public int VerbalWarnings { get; set; }

    /// <summary>
    /// Written warnings.
    /// </summary>
    public int WrittenWarnings { get; set; }

    /// <summary>
    /// Violations in last 90 days.
    /// </summary>
    public int ViolationsLast90Days { get; set; }

    /// <summary>
    /// Risk level: Low, Medium, High, Critical.
    /// </summary>
    public string RiskLevel { get; set; } = "Low";

    /// <summary>
    /// Average time to close investigation (in days).
    /// </summary>
    public double? AverageResolutionDays { get; set; }
}

using EmployeeInvestigationSystem.Application.DTOs;

namespace EmployeeInvestigationSystem.Application.Interfaces;

/// <summary>
/// Service for dashboard data and analytics.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Get complete dashboard data with all charts and statistics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard data</returns>
    Task<DashboardDto> GetDashboardDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get violations by factory breakdown.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Chart data for violations by factory</returns>
    Task<IEnumerable<ChartDataDto>> GetViolationsByFactoryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get violations by department breakdown.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Chart data for violations by department</returns>
    Task<IEnumerable<ChartDataDto>> GetViolationsByDepartmentAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get violations by case type breakdown.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Chart data for violations by type</returns>
    Task<IEnumerable<ChartDataDto>> GetViolationsByTypeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get violations by outcome breakdown.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Chart data for violations by outcome</returns>
    Task<IEnumerable<ChartDataDto>> GetViolationsByOutcomeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get 12-month violation trend data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Trend data for last 12 months</returns>
    Task<IEnumerable<TrendDataDto>> GetViolationsTrendAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top violators in the system.
    /// </summary>
    /// <param name="topCount">Number of top violators to return (default 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of top violators</returns>
    Task<IEnumerable<TopViolatorDto>> GetTopViolatorsAsync(int topCount = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent investigations.
    /// </summary>
    /// <param name="count">Number of recent investigations to return (default 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recent investigations</returns>
    Task<IEnumerable<RecentInvestigationDto>> GetRecentInvestigationsAsync(int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export dashboard report to Excel.
    /// </summary>
    /// <param name="request">Export request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Excel file bytes</returns>
    Task<byte[]> ExportToExcelAsync(ExportReportRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export dashboard report to PDF.
    /// </summary>
    /// <param name="request">Export request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF file bytes</returns>
    Task<byte[]> ExportToPdfAsync(ExportReportRequest request, CancellationToken cancellationToken = default);
}

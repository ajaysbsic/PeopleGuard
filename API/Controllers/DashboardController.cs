using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeInvestigationSystem.API.Controllers;

/// <summary>
/// Provides dashboard data, analytics, and reporting capabilities.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Business,ER,HR,ITAdmin,Management,Manager")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get complete dashboard data with all charts and statistics.
    /// </summary>
    /// <returns>Dashboard data with charts and analytics</returns>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var dashboard = await _dashboardService.GetDashboardDataAsync();
            _logger.LogInformation("Dashboard data retrieved");
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving dashboard: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve dashboard data" });
        }
    }

    /// <summary>
    /// Get violations by factory breakdown.
    /// </summary>
    /// <returns>Chart data for violations by factory</returns>
    [HttpGet("violations-by-factory")]
    [ProducesResponseType(typeof(IEnumerable<ChartDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetViolationsByFactory()
    {
        try
        {
            var data = await _dashboardService.GetViolationsByFactoryAsync();
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving violations by factory: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve data" });
        }
    }

    /// <summary>
    /// Get violations by department breakdown.
    /// </summary>
    /// <returns>Chart data for violations by department</returns>
    [HttpGet("violations-by-department")]
    [ProducesResponseType(typeof(IEnumerable<ChartDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetViolationsByDepartment()
    {
        try
        {
            var data = await _dashboardService.GetViolationsByDepartmentAsync();
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving violations by department: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve data" });
        }
    }

    /// <summary>
    /// Get violations by case type breakdown.
    /// </summary>
    /// <returns>Chart data for violations by type</returns>
    [HttpGet("violations-by-type")]
    [ProducesResponseType(typeof(IEnumerable<ChartDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetViolationsByType()
    {
        try
        {
            var data = await _dashboardService.GetViolationsByTypeAsync();
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving violations by type: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve data" });
        }
    }

    /// <summary>
    /// Get violations by outcome breakdown.
    /// </summary>
    /// <returns>Chart data for violations by outcome</returns>
    [HttpGet("violations-by-outcome")]
    [ProducesResponseType(typeof(IEnumerable<ChartDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetViolationsByOutcome()
    {
        try
        {
            var data = await _dashboardService.GetViolationsByOutcomeAsync();
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving violations by outcome: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve data" });
        }
    }

    /// <summary>
    /// Get violations trend for last 12 months.
    /// </summary>
    /// <returns>Trend data for violations over time</returns>
    [HttpGet("violations-trend")]
    [ProducesResponseType(typeof(IEnumerable<TrendDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetViolationsTrend()
    {
        try
        {
            var data = await _dashboardService.GetViolationsTrendAsync();
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving violations trend: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve data" });
        }
    }

    /// <summary>
    /// Get top violators in the system.
    /// </summary>
    /// <param name="topCount">Number of top violators to return (default 10)</param>
    /// <returns>List of employees with most violations</returns>
    [HttpGet("top-violators")]
    [ProducesResponseType(typeof(IEnumerable<TopViolatorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTopViolators([FromQuery] int topCount = 10)
    {
        try
        {
            var data = await _dashboardService.GetTopViolatorsAsync(topCount);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving top violators: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve data" });
        }
    }

    /// <summary>
    /// Get recent investigations.
    /// </summary>
    /// <param name="count">Number of recent investigations to return (default 10)</param>
    /// <returns>List of recent investigations</returns>
    [HttpGet("recent-investigations")]
    [ProducesResponseType(typeof(IEnumerable<RecentInvestigationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecentInvestigations([FromQuery] int count = 10)
    {
        try
        {
            var data = await _dashboardService.GetRecentInvestigationsAsync(count);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving recent investigations: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve data" });
        }
    }

    /// <summary>
    /// Export dashboard report.
    /// </summary>
    /// <param name="request">Export request with format and filters</param>
    /// <returns>File download (Excel or PDF)</returns>
    [HttpPost("export")]
    [Authorize(Roles = "Admin,Business,ER,HR,Management,Manager")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportReport([FromBody] ExportReportRequest request)
    {
        try
        {
            byte[] fileBytes;
            string contentType;
            string fileName;

            if (request.ReportType?.ToLower() == "pdf")
            {
                fileBytes = await _dashboardService.ExportToPdfAsync(request);
                contentType = "application/pdf";
                fileName = $"investigation_report_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            }
            else
            {
                fileBytes = await _dashboardService.ExportToExcelAsync(request);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = $"investigation_report_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            }

            _logger.LogInformation("Report exported as {Format}", request.ReportType);
            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error exporting report: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to export report" });
        }
    }

    /// <summary>
    /// Get summary dashboard data (lightweight version for quick loading)
    /// </summary>
    /// <returns>Dashboard summary with key metrics</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboardSummary()
    {
        try
        {
            var dashboard = await _dashboardService.GetDashboardDataAsync();
            _logger.LogInformation("Dashboard summary retrieved");
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving dashboard summary: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve dashboard summary" });
        }
    }}
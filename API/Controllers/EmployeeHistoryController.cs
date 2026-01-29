using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeInvestigationSystem.API.Controllers;

/// <summary>
/// Manages employee history and statistics related to investigations and violations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeHistoryController : ControllerBase
{
    private readonly IEmployeeHistoryService _historyService;
    private readonly ILogger<EmployeeHistoryController> _logger;

    public EmployeeHistoryController(IEmployeeHistoryService historyService, ILogger<EmployeeHistoryController> logger)
    {
        _historyService = historyService;
        _logger = logger;
    }

    /// <summary>
    /// Get complete history for an employee including investigations and warnings.
    /// </summary>
    /// <param name="employeeId">ID of the employee</param>
    /// <returns>Employee history with all investigations and warnings</returns>
    [HttpGet("{employeeId}")]
    [ProducesResponseType(typeof(EmployeeHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEmployeeHistory(Guid employeeId)
    {
        try
        {
            var history = await _historyService.GetEmployeeHistoryAsync(employeeId);
            _logger.LogInformation("Employee history retrieved for {EmployeeId}", employeeId);
            return Ok(history);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Employee history not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving employee history: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve employee history" });
        }
    }

    /// <summary>
    /// Get statistics for an employee including risk level and trend data.
    /// </summary>
    /// <param name="employeeId">ID of the employee</param>
    /// <returns>Employee statistics with violations, warnings, and risk level</returns>
    [HttpGet("{employeeId}/statistics")]
    [ProducesResponseType(typeof(EmployeeStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEmployeeStatistics(Guid employeeId)
    {
        try
        {
            var statistics = await _historyService.GetEmployeeStatisticsAsync(employeeId);
            _logger.LogInformation("Employee statistics retrieved for {EmployeeId}", employeeId);
            return Ok(statistics);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Employee statistics not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving employee statistics: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve employee statistics" });
        }
    }
}

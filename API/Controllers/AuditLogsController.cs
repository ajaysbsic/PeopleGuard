using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeInvestigationSystem.API.Controllers;

/// <summary>
/// Manages audit logs and system activity tracking.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,ITAdmin")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(IAuditLogService auditLogService, ILogger<AuditLogsController> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Get audit logs for a specific entity.
    /// </summary>
    /// <param name="entityId">ID of the entity to retrieve audit logs for</param>
    /// <returns>Audit logs for the entity</returns>
    [HttpGet("entity/{entityId}")]
    [ProducesResponseType(typeof(IEnumerable<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAuditLogsForEntity(string entityId)
    {
        try
        {
            var logs = await _auditLogService.GetAuditLogsForEntityAsync(entityId);
            if (!logs.Any())
            {
                return NotFound(new { message = $"No audit logs found for entity {entityId}" });
            }

            _logger.LogInformation("Audit logs retrieved for entity {EntityId}", entityId);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving audit logs for entity: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve audit logs" });
        }
    }

    /// <summary>
    /// Get audit logs for a specific user.
    /// </summary>
    /// <param name="userId">ID of the user to retrieve audit logs for</param>
    /// <returns>Audit logs for the user</returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAuditLogsForUser(Guid userId)
    {
        try
        {
            var logs = await _auditLogService.GetAuditLogsForUserAsync(userId);
            if (!logs.Any())
            {
                return NotFound(new { message = $"No audit logs found for user {userId}" });
            }

            _logger.LogInformation("Audit logs retrieved for user {UserId}", userId);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving audit logs for user: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve audit logs" });
        }
    }

    /// <summary>
    /// Get audit logs with optional filtering.
    /// </summary>
    /// <param name="startDate">Start date for filtering (optional)</param>
    /// <param name="endDate">End date for filtering (optional)</param>
    /// <param name="action">Action type to filter (optional): Create, Update, Delete</param>
    /// <returns>Filtered audit logs</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? action = null)
    {
        try
        {
            var logs = await _auditLogService.GetAuditLogsAsync(startDate, endDate, action);
            _logger.LogInformation("Audit logs retrieved with filters: StartDate={StartDate}, EndDate={EndDate}, Action={Action}",
                startDate, endDate, action);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving audit logs: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to retrieve audit logs" });
        }
    }

    /// <summary>
    /// Delete old audit logs based on retention policy.
    /// </summary>
    /// <param name="retentionDays">Number of days to retain (default 90)</param>
    /// <returns>Number of deleted audit log entries</returns>
    [HttpDelete("cleanup")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteOldAuditLogs([FromQuery] int retentionDays = 90)
    {
        try
        {
            var deletedCount = await _auditLogService.DeleteOldAuditLogsAsync(retentionDays);
            _logger.LogInformation("Old audit logs deleted: {Count} records older than {RetentionDays} days", deletedCount, retentionDays);
            return Ok(new { message = $"Deleted {deletedCount} audit log entries older than {retentionDays} days" });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting old audit logs: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to delete old audit logs" });
        }
    }
}

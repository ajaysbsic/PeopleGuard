using EmployeeInvestigationSystem.Application.DTOs;

namespace EmployeeInvestigationSystem.Application.Interfaces;

/// <summary>
/// Service for audit logging and retrieval.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Log an action.
    /// </summary>
    /// <param name="auditLog">Audit log DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created audit log ID</returns>
    Task<Guid> LogActionAsync(AuditLogDto auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get audit logs for an entity.
    /// </summary>
    /// <param name="entityId">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit logs</returns>
    Task<IEnumerable<AuditLogDto>> GetAuditLogsForEntityAsync(string entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get audit logs for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit logs</returns>
    Task<IEnumerable<AuditLogDto>> GetAuditLogsForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all audit logs with optional filtering.
    /// </summary>
    /// <param name="startDate">Start date filter</param>
    /// <param name="endDate">End date filter</param>
    /// <param name="action">Action filter (Create, Read, Update, Delete)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit logs</returns>
    Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? action = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete old audit logs (retention policy).
    /// </summary>
    /// <param name="retentionDays">Number of days to retain</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of deleted records</returns>
    Task<int> DeleteOldAuditLogsAsync(int retentionDays = 90, CancellationToken cancellationToken = default);
}

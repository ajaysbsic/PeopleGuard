namespace EmployeeInvestigationSystem.Application.DTOs;

/// <summary>
/// Audit log entry for tracking changes.
/// </summary>
public class AuditLogDto
{
    /// <summary>
    /// Unique audit log ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User ID who performed the action.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User name/email.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Entity type affected.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Entity ID affected.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Action performed: Create, Read, Update, Delete.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Previous values (JSON).
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// New values (JSON).
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// API endpoint called.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// HTTP method.
    /// </summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Timestamp of the action.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// IP address of the requester.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes about the action.
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Audit log filter request for querying logs with date/user/action filters
/// </summary>
public class AuditFilterRequest
{
    public string? UserName { get; set; }
    public string? Action { get; set; }
    public string? EntityType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 20;
}

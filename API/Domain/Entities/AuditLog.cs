using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeInvestigationSystem.Domain.Entities;

/// <summary>
/// Audit log entry for tracking system changes.
/// </summary>
[Table("AuditLogs")]
public class AuditLog
{
    /// <summary>
    /// Unique audit log ID.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// User ID who performed the action.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User name/email.
    /// </summary>
    [StringLength(256)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Entity type affected.
    /// </summary>
    [StringLength(128)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Entity ID affected.
    /// </summary>
    [StringLength(256)]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Action performed: Create, Read, Update, Delete.
    /// </summary>
    [StringLength(50)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Previous values in JSON format.
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// New values in JSON format.
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// API endpoint called.
    /// </summary>
    [StringLength(256)]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// HTTP method.
    /// </summary>
    [StringLength(10)]
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Timestamp of the action (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// IP address of the requester.
    /// </summary>
    [StringLength(45)] // IPv6 max length
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Request duration in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Additional notes about the action.
    /// </summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}

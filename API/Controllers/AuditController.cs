using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeInvestigationSystem.API.Controllers;

/// <summary>
/// Audit log viewer for admins (read-only with sensitive field masking)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,ITAdmin")]
public class AuditController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuditController> _logger;

    public AuditController(AppDbContext context, ILogger<AuditController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get audit logs with filters and paging
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? userName,
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        size = Math.Clamp(size, 1, 100);

        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(userName))
            query = query.Where(a => a.UserName.Contains(userName));

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action.Contains(action));

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType.Contains(entityType));

        var fromDate = from ?? new DateTime(1753, 1, 1);
        var toDate = to ?? new DateTime(9999, 12, 31);

        query = query.Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate);

        var total = await query.CountAsync(cancellationToken);

        var logsRaw = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        var logs = logsRaw
            .Select(a => new
            {
                a.Id,
                a.UserName,
                a.EntityType,
                a.EntityId,
                a.Action,
                a.Endpoint,
                a.HttpMethod,
                a.StatusCode,
                IpAddress = MaskIpAddress(a.IpAddress),
                a.Timestamp,
                Details = MaskSensitiveFields(a.OldValues, a.NewValues)
            })
            .ToList();

        return Ok(new
        {
            data = logs,
            page,
            size,
            total,
            totalPages = Math.Ceiling((double)total / size)
        });
    }

    /// <summary>
    /// Export audit logs as CSV
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportAuditLogs(
        [FromQuery] string? userName,
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(userName))
            query = query.Where(a => a.UserName.Contains(userName));

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action.Contains(action));

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType.Contains(entityType));

        var fromDate = from ?? new DateTime(1753, 1, 1);
        var toDate = to ?? new DateTime(9999, 12, 31);

        query = query.Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate);

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Take(10000) // limit to 10k rows
            .ToListAsync(cancellationToken);

        var csv = GenerateCsv(logs);
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);

        return File(bytes, "text/csv", $"audit-export-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private string MaskIpAddress(string? ip)
    {
        if (string.IsNullOrEmpty(ip)) return "N/A";
        var parts = ip.Split('.');
        if (parts.Length != 4) return ip;
        return $"{parts[0]}.{parts[1]}.*.* ";
    }

    private string MaskSensitiveFields(string? old, string? newVal)
    {
        // Simple masking: replace common sensitive field values
        var masked = old ?? "";
        masked = System.Text.RegularExpressions.Regex.Replace(masked, @"""password""\s*:\s*""[^""]*""", "\"password\": \"***\"");
        masked = System.Text.RegularExpressions.Regex.Replace(masked, @"""token""\s*:\s*""[^""]*""", "\"token\": \"***\"");
        masked = System.Text.RegularExpressions.Regex.Replace(masked, @"""email""\s*:\s*""[^""]*""", "\"email\": \"***@***.***\"");
        return masked;
    }

    private string GenerateCsv(List<Domain.Entities.AuditLog> logs)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("\"User\",\"Entity Type\",\"Entity ID\",\"Action\",\"Endpoint\",\"HTTP Method\",\"Status\",\"IP Address\",\"Timestamp\"");

        foreach (var log in logs)
        {
            sb.AppendLine($"\"{EscapeCsv(log.UserName)}\",\"{EscapeCsv(log.EntityType)}\",\"{EscapeCsv(log.EntityId)}\",\"{EscapeCsv(log.Action)}\",\"{EscapeCsv(log.Endpoint)}\",\"{log.HttpMethod}\",\"{log.StatusCode}\",\"{MaskIpAddress(log.IpAddress)}\",\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\"");
        }

        return sb.ToString();
    }

    private string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\"", "\"\"");
    }
}

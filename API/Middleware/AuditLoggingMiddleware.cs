using System.Security.Claims;
using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Application.Interfaces;

namespace EmployeeInvestigationSystem.API.Middleware;

/// <summary>
/// Middleware for logging all HTTP requests for audit purposes.
/// </summary>
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    // Routes to audit
    private static readonly HashSet<string> AuditedRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/employees",
        "/api/investigations",
        "/api/warnings",
        "/api/warningletters",
        "/api/leaves"
    };

    // Methods to audit (exclude GET for performance)
    private static readonly HashSet<string> AuditedMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "POST",
        "PUT",
        "PATCH",
        "DELETE"
    };

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
    {
        var startTime = DateTime.UtcNow;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Capture original response body
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            stopwatch.Stop();

            // Check if this request should be audited
            if (ShouldAudit(context))
            {
                await LogAuditAsync(context, auditLogService, startTime, stopwatch.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error in audit logging middleware");
            throw;
        }
        finally
        {
            // Reset stream position and copy response body back to original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private bool ShouldAudit(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var method = context.Request.Method.ToUpperInvariant();

        // Check if route should be audited
        var shouldAuditRoute = AuditedRoutes.Any(route => path.Contains(route));
        if (!shouldAuditRoute)
            return false;

        // Check if method should be audited
        return AuditedMethods.Contains(method);
    }

    private async Task LogAuditAsync(HttpContext context, IAuditLogService auditLogService, DateTime timestamp, long durationMs)
    {
        try
        {
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            var userName = context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

            var auditLog = new AuditLogDto
            {
                UserId = Guid.TryParse(userId, out var parsedId) ? parsedId : Guid.Empty,
                UserName = userName,
                EntityType = ExtractEntityType(context.Request.Path),
                EntityId = ExtractEntityId(context.Request.Path),
                Action = context.Request.Method.ToUpper(),
                Endpoint = context.Request.Path.Value,
                HttpMethod = context.Request.Method,
                StatusCode = context.Response.StatusCode,
                Timestamp = timestamp,
                IpAddress = GetClientIpAddress(context)
            };

            // Try to extract request/response bodies for JSON APIs
            if (context.Request.Method != "GET" && context.Response.StatusCode < 400)
            {
                auditLog.Notes = $"Request processed successfully";
            }
            else if (context.Response.StatusCode >= 400)
            {
                auditLog.Notes = $"Request failed with status {context.Response.StatusCode}";
            }

            await auditLogService.LogActionAsync(auditLog);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log audit entry");
            // Don't throw - audit logging should not break the request
        }
    }

    private static string ExtractEntityType(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant() ?? "";
        
        if (pathValue.Contains("/employees"))
            return "Employee";
        if (pathValue.Contains("/investigations"))
            return "Investigation";
        if (pathValue.Contains("/warnings") || pathValue.Contains("/warningletters"))
            return "WarningLetter";
        if (pathValue.Contains("/leaves"))
            return "LeaveRequest";

        return "Unknown";
    }

    private static string ExtractEntityId(PathString path)
    {
        var segments = path.Value?.Split('/') ?? Array.Empty<string>();
        
        // Try to get ID from URL pattern like /api/employees/{id}
        for (int i = segments.Length - 1; i >= 0; i--)
        {
            var segment = segments[i];
            if (Guid.TryParse(segment, out _))
                return segment;
        }

        return "Unknown";
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            return forwardedFor.ToString().Split(',')[0];

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}

/// <summary>
/// Extension methods for audit logging middleware.
/// </summary>
public static class AuditLoggingMiddlewareExtensions
{
    /// <summary>
    /// Add audit logging middleware to the pipeline.
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder</returns>
    public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuditLoggingMiddleware>();
    }
}

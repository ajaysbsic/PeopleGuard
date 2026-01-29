using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Domain.Entities;
using EmployeeInvestigationSystem.Domain.Enums;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace EmployeeInvestigationSystem.API.Controllers;

/// <summary>
/// Cases controller - provides paginated, filterable case listing
/// This is a view-oriented controller that joins investigations with employee data
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CasesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<CasesController> _logger;

    public CasesController(AppDbContext context, ILogger<CasesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of cases with filters
    /// GET /api/cases?employeeId=&factory=&type=&status=&from=&to=&page=1&size=20
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<CaseListItemDto>>> GetCases(
        [FromQuery] string? employeeId,
        [FromQuery] string? factory,
        [FromQuery] int? type,
        [FromQuery] int? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination
        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 100);

        // Build query
        var query = _context.Investigations
            .AsNoTracking()
            .Where(i => !i.IsDeleted)
            .Join(
                _context.Employees.Where(e => !e.IsDeleted),
                i => i.EmployeeId,
                e => e.Id,
                (i, e) => new { Investigation = i, Employee = e }
            );

        // Apply filters
        if (!string.IsNullOrWhiteSpace(employeeId))
        {
            query = query.Where(x => 
                x.Employee.EmployeeId.Contains(employeeId) || 
                x.Employee.Name.Contains(employeeId));
        }

        if (!string.IsNullOrWhiteSpace(factory))
        {
            query = query.Where(x => x.Employee.Factory == factory);
        }

        if (type.HasValue)
        {
            var caseType = (InvestigationCaseType)type.Value;
            query = query.Where(x => x.Investigation.CaseType == caseType);
        }

        if (status.HasValue)
        {
            var investStatus = (InvestigationStatus)status.Value;
            query = query.Where(x => x.Investigation.Status == investStatus);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.Investigation.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            var toEndOfDay = to.Value.Date.AddDays(1);
            query = query.Where(x => x.Investigation.CreatedAt < toEndOfDay);
        }

        // Get total count
        var total = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = sortBy?.ToLowerInvariant() switch
        {
            "caseid" => sortDesc 
                ? query.OrderByDescending(x => x.Investigation.Id) 
                : query.OrderBy(x => x.Investigation.Id),
            "employee" => sortDesc 
                ? query.OrderByDescending(x => x.Employee.Name) 
                : query.OrderBy(x => x.Employee.Name),
            "factory" => sortDesc 
                ? query.OrderByDescending(x => x.Employee.Factory) 
                : query.OrderBy(x => x.Employee.Factory),
            "type" => sortDesc 
                ? query.OrderByDescending(x => x.Investigation.CaseType) 
                : query.OrderBy(x => x.Investigation.CaseType),
            "status" => sortDesc 
                ? query.OrderByDescending(x => x.Investigation.Status) 
                : query.OrderBy(x => x.Investigation.Status),
            "updated" => sortDesc 
                ? query.OrderByDescending(x => x.Investigation.ClosedAt ?? x.Investigation.CreatedAt) 
                : query.OrderBy(x => x.Investigation.ClosedAt ?? x.Investigation.CreatedAt),
            _ => sortDesc 
                ? query.OrderByDescending(x => x.Investigation.CreatedAt) 
                : query.OrderBy(x => x.Investigation.CreatedAt)
        };

        // If default (no sortBy), sort by newest first
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            query = query.OrderByDescending(x => x.Investigation.CreatedAt);
        }

        // Apply pagination
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .Select(x => new CaseListItemDto
            {
                Id = x.Investigation.Id,
                CaseId = GenerateCaseId(x.Investigation.Id, x.Investigation.CreatedAt),
                EmployeeId = x.Employee.Id,
                EmployeeName = x.Employee.Name,
                EmployeeCode = x.Employee.EmployeeId,
                Factory = x.Employee.Factory,
                Department = x.Employee.Department,
                CaseType = (int)x.Investigation.CaseType,
                CaseTypeName = GetCaseTypeName(x.Investigation.CaseType),
                Status = (int)x.Investigation.Status,
                StatusName = GetStatusName(x.Investigation.Status),
                CreatedAt = x.Investigation.CreatedAt,
                UpdatedAt = x.Investigation.ClosedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(new PagedResponse<CaseListItemDto>
        {
            Data = items,
            Page = page,
            Size = size,
            Total = total
        });
    }

    /// <summary>
    /// Get available factories for filter dropdown
    /// </summary>
    [HttpGet("factories")]
    public async Task<ActionResult<IEnumerable<string>>> GetFactories(CancellationToken cancellationToken)
    {
        var factories = await _context.Employees
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Select(e => e.Factory)
            .Distinct()
            .OrderBy(f => f)
            .ToListAsync(cancellationToken);

        return Ok(factories);
    }

    /// <summary>
    /// Get case statistics for dashboard
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<CaseStatsDto>> GetStats(CancellationToken cancellationToken)
    {
        var stats = await _context.Investigations
            .AsNoTracking()
            .Where(i => !i.IsDeleted)
            .GroupBy(_ => 1)
            .Select(g => new CaseStatsDto
            {
                Total = g.Count(),
                Open = g.Count(i => i.Status == InvestigationStatus.Open),
                UnderInvestigation = g.Count(i => i.Status == InvestigationStatus.UnderInvestigation),
                Closed = g.Count(i => i.Status == InvestigationStatus.Closed)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return Ok(stats ?? new CaseStatsDto());
    }

    private static string GenerateCaseId(Guid id, DateTime createdAt)
    {
        // Generate a readable case ID: C-YYYY-XXXX (last 4 chars of GUID)
        var shortId = id.ToString("N")[..4].ToUpper();
        return $"C-{createdAt.Year}-{shortId}";
    }

    private static string GetCaseTypeName(InvestigationCaseType type) => type switch
    {
        InvestigationCaseType.Violation => "Violation",
        InvestigationCaseType.Safety => "Safety Issue",
        InvestigationCaseType.Misbehavior => "Misbehavior",
        InvestigationCaseType.Investigation => "Investigation",
        _ => "Unknown"
    };

    private static string GetStatusName(InvestigationStatus status) => status switch
    {
        InvestigationStatus.Open => "Open",
        InvestigationStatus.UnderInvestigation => "Under Investigation",
        InvestigationStatus.Closed => "Closed",
        _ => "Unknown"
    };

    private static string GetOutcomeName(WarningOutcome? outcome) => outcome switch
    {
        WarningOutcome.NoAction => "No Action",
        WarningOutcome.VerbalWarning => "Verbal Warning",
        WarningOutcome.WrittenWarning => "Written Warning",
        _ => null
    } ?? string.Empty;

    private static string GetEventTypeName(CaseHistoryEventType type) => type switch
    {
        CaseHistoryEventType.Created => "Case Created",
        CaseHistoryEventType.StatusChanged => "Status Changed",
        CaseHistoryEventType.RemarkAdded => "Remark Added",
        CaseHistoryEventType.AttachmentAdded => "Attachment Added",
        CaseHistoryEventType.AttachmentRemoved => "Attachment Removed",
        CaseHistoryEventType.WarningLetterIssued => "Warning Letter Issued",
        CaseHistoryEventType.OutcomeSet => "Outcome Set",
        _ => "Unknown"
    };

    private static WarningOutcome MapOutcome(string outcome) => outcome.ToLowerInvariant() switch
    {
        "none" or "noaction" => WarningOutcome.NoAction,
        "verbalwarning" => WarningOutcome.VerbalWarning,
        "writtenwarning" => WarningOutcome.WrittenWarning,
        _ => WarningOutcome.NoAction
    };

    private static string SanitizeHtml(string html)
    {
        // Remove script tags and on* attributes
        var withoutScripts = Regex.Replace(html, "<script[^>]*?>.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var withoutEvents = Regex.Replace(withoutScripts, "on\\w+\\s*=\\s*\".*?\"", string.Empty, RegexOptions.IgnoreCase);
        withoutEvents = Regex.Replace(withoutEvents, "on\\w+\\s*=\\s*'.*?'", string.Empty, RegexOptions.IgnoreCase);
        return withoutEvents;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private string GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value 
            ?? User.FindFirst(ClaimTypes.Email)?.Value 
            ?? "System";
    }

    /// <summary>
    /// Get case detail by ID
    /// GET /api/cases/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CaseDetailDto>> GetCaseById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _context.Investigations
            .AsNoTracking()
            .Where(i => i.Id == id && !i.IsDeleted)
            .Join(
                _context.Employees.Where(e => !e.IsDeleted),
                i => i.EmployeeId,
                e => e.Id,
                (i, e) => new { Investigation = i, Employee = e }
            )
            .Select(x => new CaseDetailDto
            {
                Id = x.Investigation.Id,
                CaseId = GenerateCaseId(x.Investigation.Id, x.Investigation.CreatedAt),
                EmployeeId = x.Employee.Id,
                EmployeeName = x.Employee.Name,
                EmployeeCode = x.Employee.EmployeeId,
                Factory = x.Employee.Factory,
                Department = x.Employee.Department,
                Designation = x.Employee.Designation,
                Title = x.Investigation.Title,
                Description = x.Investigation.Description,
                CaseType = (int)x.Investigation.CaseType,
                CaseTypeName = GetCaseTypeName(x.Investigation.CaseType),
                Status = (int)x.Investigation.Status,
                StatusName = GetStatusName(x.Investigation.Status),
                Outcome = x.Investigation.Outcome.HasValue ? (int?)x.Investigation.Outcome.Value : null,
                OutcomeName = GetOutcomeName(x.Investigation.Outcome),
                CreatedAt = x.Investigation.CreatedAt,
                ClosedAt = x.Investigation.ClosedAt,
                RemarksCount = x.Investigation.Remarks.Count,
                AttachmentsCount = x.Investigation.Attachments.Count,
                HasWarningLetter = _context.WarningLetters.Any(w => w.InvestigationId == x.Investigation.Id),
                WarningLetterId = _context.WarningLetters
                    .Where(w => w.InvestigationId == x.Investigation.Id)
                    .OrderByDescending(w => w.IssuedAt)
                    .Select(w => (Guid?)w.Id)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
            return NotFound(new { message = "Case not found" });

        return Ok(result);
    }

    /// <summary>
    /// Get case history/timeline
    /// GET /api/cases/{id}/history
    /// </summary>
    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<IEnumerable<CaseHistoryDto>>> GetCaseHistory(
        Guid id, 
        CancellationToken cancellationToken)
    {
        var caseExists = await _context.Investigations
            .AsNoTracking()
            .AnyAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);

        if (!caseExists)
            return NotFound(new { message = "Case not found" });

        var history = await _context.CaseHistory
            .AsNoTracking()
            .Where(h => h.InvestigationId == id)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new CaseHistoryDto
            {
                Id = h.Id,
                EventType = (int)h.EventType,
                EventTypeName = GetEventTypeName(h.EventType),
                Description = h.Description,
                OldValue = h.OldValue,
                NewValue = h.NewValue,
                ReferenceId = h.ReferenceId,
                UserId = h.UserId,
                UserName = h.User != null ? h.User.UserName ?? h.User.Email : "System",
                CreatedAt = h.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(history);
    }

    /// <summary>
    /// Get case attachments
    /// GET /api/cases/{id}/attachments
    /// </summary>
    [HttpGet("{id:guid}/attachments")]
    public async Task<ActionResult<IEnumerable<CaseAttachmentDto>>> GetCaseAttachments(
        Guid id, 
        CancellationToken cancellationToken)
    {
        var caseExists = await _context.Investigations
            .AsNoTracking()
            .AnyAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);

        if (!caseExists)
            return NotFound(new { message = "Case not found" });

        var attachments = await _context.InvestigationAttachments
            .AsNoTracking()
            .Where(a => a.InvestigationId == id)
            .OrderByDescending(a => a.UploadedAt)
            .Select(a => new CaseAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                UploadedAt = a.UploadedAt,
                DownloadUrl = $"/api/cases/{id}/attachments/{a.Id}/download"
            })
            .ToListAsync(cancellationToken);

        return Ok(attachments);
    }

    /// <summary>
    /// Get case remarks
    /// GET /api/cases/{id}/remarks
    /// </summary>
    [HttpGet("{id:guid}/remarks")]
    public async Task<ActionResult<IEnumerable<CaseRemarkDto>>> GetCaseRemarks(
        Guid id, 
        CancellationToken cancellationToken)
    {
        var caseExists = await _context.Investigations
            .AsNoTracking()
            .AnyAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);

        if (!caseExists)
            return NotFound(new { message = "Case not found" });

        var remarks = await _context.InvestigationRemarks
            .AsNoTracking()
            .Where(r => r.InvestigationId == id)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new CaseRemarkDto
            {
                Id = r.Id,
                Text = r.Remark,
                UserId = r.UserId,
                UserName = r.User != null ? r.User.UserName ?? r.User.Email ?? "Unknown" : "Unknown",
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(remarks);
    }

    /// <summary>
    /// Download attachment
    /// GET /api/cases/{id}/attachments/{attachmentId}/download
    /// </summary>
    [HttpGet("{id:guid}/attachments/{attachmentId:guid}/download")]
    public async Task<IActionResult> DownloadAttachment(
        Guid id, 
        Guid attachmentId, 
        CancellationToken cancellationToken)
    {
        var attachment = await _context.InvestigationAttachments
            .AsNoTracking()
            .Where(a => a.Id == attachmentId && a.InvestigationId == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (attachment == null)
            return NotFound(new { message = "Attachment not found" });

        if (!System.IO.File.Exists(attachment.FilePath))
            return NotFound(new { message = "File not found on server" });

        var fileBytes = await System.IO.File.ReadAllBytesAsync(attachment.FilePath, cancellationToken);
        return File(fileBytes, attachment.ContentType, attachment.FileName);
    }

    /// <summary>
    /// Update case status - ER/HR only
    /// PATCH /api/cases/{id}/status
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,ER,HR")]
    public async Task<ActionResult<CaseDetailDto>> UpdateCaseStatus(
        Guid id,
        [FromBody] UpdateCaseStatusRequest request,
        CancellationToken cancellationToken)
    {
        var investigation = await _context.Investigations
            .Where(i => i.Id == id && !i.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (investigation == null)
            return NotFound(new { message = "Case not found" });

        // Parse new status
        if (!Enum.TryParse<InvestigationStatus>(request.Status, true, out var newStatus))
            return BadRequest(new { message = "Invalid status value. Use: Open, UnderInvestigation, Closed" });

        var oldStatus = investigation.Status;

        // Validate status transitions
        var validTransition = (oldStatus, newStatus) switch
        {
            (InvestigationStatus.Open, InvestigationStatus.UnderInvestigation) => true,
            (InvestigationStatus.Open, InvestigationStatus.Closed) => investigation.Outcome.HasValue || request.Outcome.HasValue,
            (InvestigationStatus.UnderInvestigation, InvestigationStatus.Closed) => true,
            (InvestigationStatus.UnderInvestigation, InvestigationStatus.Open) => true, // Allow reopening
            (InvestigationStatus.Closed, InvestigationStatus.Open) => true, // Allow reopening
            _ when oldStatus == newStatus => true, // No change
            _ => false
        };

        if (!validTransition)
        {
            if (newStatus == InvestigationStatus.Closed && !investigation.Outcome.HasValue && !request.Outcome.HasValue)
                return BadRequest(new { message = "Cannot close case without setting an outcome" });
            return BadRequest(new { message = $"Invalid status transition from {oldStatus} to {newStatus}" });
        }

        // Set outcome if provided
        if (request.Outcome.HasValue)
        {
            investigation.Outcome = (WarningOutcome)request.Outcome.Value;
        }

        // Update status
        investigation.Status = newStatus;
        if (newStatus == InvestigationStatus.Closed)
        {
            investigation.ClosedAt = DateTime.UtcNow;
        }
        else if (oldStatus == InvestigationStatus.Closed)
        {
            investigation.ClosedAt = null; // Reopened
        }

        // Add history entry
        var historyEntry = new CaseHistory
        {
            Id = Guid.NewGuid(),
            InvestigationId = id,
            UserId = GetCurrentUserId(),
            EventType = CaseHistoryEventType.StatusChanged,
            Description = $"Status changed from {GetStatusName(oldStatus)} to {GetStatusName(newStatus)}",
            OldValue = oldStatus.ToString(),
            NewValue = newStatus.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        _context.CaseHistory.Add(historyEntry);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case {CaseId} status updated from {OldStatus} to {NewStatus} by {User}",
            id, oldStatus, newStatus, GetCurrentUserName());

        return await GetCaseById(id, cancellationToken);
    }

    /// <summary>
    /// Add remark to case - ER/HR only
    /// POST /api/cases/{id}/remarks
    /// </summary>
    [HttpPost("{id:guid}/remarks")]
    [Authorize(Roles = "Admin,ER,HR")]
    public async Task<ActionResult<CaseRemarkDto>> AddRemark(
        Guid id,
        [FromBody] AddRemarkRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new { message = "Remark text is required" });

        var investigation = await _context.Investigations
            .Where(i => i.Id == id && !i.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (investigation == null)
            return NotFound(new { message = "Case not found" });

        if (investigation.Status == InvestigationStatus.Closed)
            return BadRequest(new { message = "Case is closed and cannot be modified" });

        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();

        var remark = new InvestigationRemark
        {
            Id = Guid.NewGuid(),
            InvestigationId = id,
            UserId = userId,
            Remark = request.Text,
            CreatedAt = DateTime.UtcNow
        };
        _context.InvestigationRemarks.Add(remark);

        // Add history entry
        var historyEntry = new CaseHistory
        {
            Id = Guid.NewGuid(),
            InvestigationId = id,
            UserId = userId,
            EventType = CaseHistoryEventType.RemarkAdded,
            Description = request.Text.Length > 100 ? request.Text[..100] + "..." : request.Text,
            ReferenceId = remark.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.CaseHistory.Add(historyEntry);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Remark added to case {CaseId} by {User}", id, userName);

        return Ok(new CaseRemarkDto
        {
            Id = remark.Id,
            Text = remark.Remark,
            UserId = userId,
            UserName = userName,
            CreatedAt = remark.CreatedAt
        });
    }

    /// <summary>
    /// Upload attachment to case - ER/HR only
    /// POST /api/cases/{id}/attachments
    /// </summary>
    [HttpPost("{id:guid}/attachments")]
    [Authorize(Roles = "Admin,ER,HR")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<ActionResult<CaseAttachmentDto>> UploadAttachment(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });

        var investigation = await _context.Investigations
            .Where(i => i.Id == id && !i.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (investigation == null)
            return NotFound(new { message = "Case not found" });

        if (investigation.Status == InvestigationStatus.Closed)
            return BadRequest(new { message = "Case is closed and cannot be modified" });

        // Validate file type
        var allowedTypes = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedTypes.Contains(extension))
            return BadRequest(new { message = $"File type {extension} is not allowed" });

        // Save file
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "cases", id.ToString());
        Directory.CreateDirectory(uploadsDir);
        
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDir, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();

        var attachment = new InvestigationAttachment
        {
            Id = Guid.NewGuid(),
            InvestigationId = id,
            FileName = file.FileName,
            FilePath = filePath,
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadedAt = DateTime.UtcNow
        };
        _context.InvestigationAttachments.Add(attachment);

        // Add history entry
        var historyEntry = new CaseHistory
        {
            Id = Guid.NewGuid(),
            InvestigationId = id,
            UserId = userId,
            EventType = CaseHistoryEventType.AttachmentAdded,
            Description = $"Attachment added: {file.FileName}",
            ReferenceId = attachment.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.CaseHistory.Add(historyEntry);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Attachment {FileName} added to case {CaseId} by {User}", 
            file.FileName, id, userName);

        return Ok(new CaseAttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            UploadedAt = attachment.UploadedAt,
            DownloadUrl = $"/api/cases/{id}/attachments/{attachment.Id}/download"
        });
    }

    /// <summary>
    /// Delete attachment from case - ER/HR only
    /// DELETE /api/cases/{id}/attachments/{attachmentId}
    /// </summary>
    [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
    [Authorize(Roles = "Admin,ER,HR")]
    public async Task<IActionResult> DeleteAttachment(
        Guid id,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var attachment = await _context.InvestigationAttachments
            .Where(a => a.Id == attachmentId && a.InvestigationId == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (attachment == null)
            return NotFound(new { message = "Attachment not found" });

        var status = await _context.Investigations
            .Where(i => i.Id == id)
            .Select(i => i.Status)
            .FirstOrDefaultAsync(cancellationToken);

        if (status == InvestigationStatus.Closed)
            return BadRequest(new { message = "Case is closed and cannot be modified" });

        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();
        var fileName = attachment.FileName;

        // Delete file from disk
        if (System.IO.File.Exists(attachment.FilePath))
        {
            System.IO.File.Delete(attachment.FilePath);
        }

        _context.InvestigationAttachments.Remove(attachment);

        // Add history entry
        var historyEntry = new CaseHistory
        {
            Id = Guid.NewGuid(),
            InvestigationId = id,
            UserId = userId,
            EventType = CaseHistoryEventType.AttachmentRemoved,
            Description = $"Attachment removed: {fileName}",
            CreatedAt = DateTime.UtcNow
        };
        _context.CaseHistory.Add(historyEntry);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Attachment {FileName} removed from case {CaseId} by {User}",
            fileName, id, userName);

        return NoContent();
    }

    /// <summary>
    /// Set case outcome / final decision
    /// POST /api/cases/{id}/outcome
    /// </summary>
    [HttpPost("{id:guid}/outcome")]
    [Authorize(Roles = "Admin,ER,HR")]
    public async Task<IActionResult> SetOutcome(
        Guid id,
        [FromBody] SetOutcomeRequest request,
        CancellationToken cancellationToken)
    {
        var investigation = await _context.Investigations
            .Where(i => i.Id == id && !i.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (investigation == null)
            return NotFound(new { message = "Case not found" });

        if (investigation.Status == InvestigationStatus.Closed)
            return BadRequest(new { message = "Case is already closed" });

        var outcome = MapOutcome(request.Outcome);
        investigation.Outcome = outcome;

        var description = $"Outcome set to {GetOutcomeName(outcome)}";
        if (!string.IsNullOrWhiteSpace(request.FinalNote))
        {
            description += $". Note: {request.FinalNote}";
        }

        var historyEntry = new CaseHistory
        {
            Id = Guid.NewGuid(),
            InvestigationId = id,
            UserId = GetCurrentUserId(),
            EventType = CaseHistoryEventType.OutcomeSet,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
        _context.CaseHistory.Add(historyEntry);

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { outcomeId = (int)outcome });
    }

    /// <summary>
    /// Generate warning letter (standard or manual)
    /// POST /api/cases/{id}/letters
    /// </summary>
    [HttpPost("{id:guid}/letters")]
    [Authorize(Roles = "Admin,ER,HR")]
    public async Task<IActionResult> GenerateLetter(
        Guid id,
        [FromBody] GenerateLetterRequest request,
        CancellationToken cancellationToken)
    {
        var investigation = await _context.Investigations
            .Where(i => i.Id == id && !i.IsDeleted)
            .Join(
                _context.Employees,
                i => i.EmployeeId,
                e => e.Id,
                (i, e) => new { Investigation = i, Employee = e }
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (investigation == null)
            return NotFound(new { message = "Case not found" });

        if (investigation.Investigation.Status == InvestigationStatus.Closed)
            return BadRequest(new { message = "Case is closed and cannot be modified" });

        var template = request.Template?.ToLowerInvariant() == "manual" ? "manual" : "standard";

        string htmlContent;
        if (template == "standard")
        {
            var caseId = GenerateCaseId(investigation.Investigation.Id, investigation.Investigation.CreatedAt);
            htmlContent = $"" +
                "<html><head><style>body{font-family:Arial,sans-serif;line-height:1.6;padding:24px;}" +
                "h1{margin-bottom:12px;} .meta{color:#4b5563;font-size:13px;} .section{margin-top:16px;}" +
                "</style></head><body>" +
                $"<h1>Warning Letter</h1>" +
                $"<div class='meta'>Date: {DateTime.UtcNow:yyyy-MM-dd}</div>" +
                $"<div class='meta'>Case: {caseId}</div>" +
                $"<div class='meta'>Employee: {investigation.Employee.Name} ({investigation.Employee.EmployeeId})</div>" +
                $"<div class='meta'>Factory: {investigation.Employee.Factory}</div>" +
                $"<div class='meta'>Case Type: {GetCaseTypeName(investigation.Investigation.CaseType)}</div>" +
                "<div class='section'><strong>Summary</strong><br/>" +
                WebUtility.HtmlEncode(investigation.Investigation.Description) + "</div>" +
                "<div class='section'>Please acknowledge receipt of this letter.</div>" +
                "</body></html>";
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.Html))
                return BadRequest(new { message = "HTML content is required for manual template" });

            htmlContent = SanitizeHtml(request.Html);
        }

        // Persist file
        var lettersDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "letters", id.ToString());
        Directory.CreateDirectory(lettersDir);
        var fileName = $"WarningLetter-{DateTime.UtcNow:yyyyMMddHHmmss}.html";
        var filePath = Path.Combine(lettersDir, fileName);
        await System.IO.File.WriteAllTextAsync(filePath, htmlContent, cancellationToken);

        var letter = new WarningLetter
        {
            Id = Guid.NewGuid(),
            InvestigationId = investigation.Investigation.Id,
            EmployeeId = investigation.Employee.Id,
            Outcome = investigation.Investigation.Outcome ?? WarningOutcome.NoAction,
            Template = template,
            LetterContent = investigation.Investigation.Description,
            HtmlContent = htmlContent,
            PdfPath = filePath,
            IssuedAt = DateTime.UtcNow
        };
        _context.WarningLetters.Add(letter);

        // Also add as attachment for visibility
        var attachment = new InvestigationAttachment
        {
            Id = Guid.NewGuid(),
            InvestigationId = id,
            FileName = fileName,
            FilePath = filePath,
            ContentType = "text/html",
            FileSize = new FileInfo(filePath).Length,
            UploadedAt = DateTime.UtcNow
        };
        _context.InvestigationAttachments.Add(attachment);

        // History entry
        var historyEntry = new CaseHistory
        {
            Id = Guid.NewGuid(),
            InvestigationId = id,
            UserId = GetCurrentUserId(),
            EventType = CaseHistoryEventType.WarningLetterIssued,
            Description = $"Warning letter generated ({template})",
            ReferenceId = letter.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.CaseHistory.Add(historyEntry);

        await _context.SaveChangesAsync(cancellationToken);

        var pdfUrl = $"/api/cases/{id}/letters/{letter.Id}/download";
        return Ok(new { letterId = letter.Id, pdfUrl });
    }

    /// <summary>
    /// List warning letters for a case
    /// </summary>
    [HttpGet("{id:guid}/letters")]
    public async Task<ActionResult<IEnumerable<CaseLetterDto>>> GetLetters(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _context.Investigations
            .AnyAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);

        if (!exists)
            return NotFound(new { message = "Case not found" });

        var letters = await _context.WarningLetters
            .AsNoTracking()
            .Where(l => l.InvestigationId == id)
            .OrderByDescending(l => l.IssuedAt)
            .Select(l => new CaseLetterDto
            {
                Id = l.Id,
                Template = l.Template,
                Outcome = (int)l.Outcome,
                IssuedAt = l.IssuedAt,
                PdfUrl = $"/api/cases/{id}/letters/{l.Id}/download"
            })
            .ToListAsync(cancellationToken);

        return Ok(letters);
    }

    /// <summary>
    /// Download warning letter
    /// </summary>
    [HttpGet("{id:guid}/letters/{letterId:guid}/download")]
    public async Task<IActionResult> DownloadLetter(Guid id, Guid letterId, CancellationToken cancellationToken)
    {
        var letter = await _context.WarningLetters
            .Where(l => l.Id == letterId && l.InvestigationId == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (letter == null)
            return NotFound(new { message = "Letter not found" });

        if (!System.IO.File.Exists(letter.PdfPath))
            return NotFound(new { message = "File not found on server" });

        var bytes = await System.IO.File.ReadAllBytesAsync(letter.PdfPath, cancellationToken);
        var downloadName = Path.GetFileName(letter.PdfPath);
        return File(bytes, "text/html", downloadName);
    }
}

public record CaseStatsDto
{
    public int Total { get; init; }
    public int Open { get; init; }
    public int UnderInvestigation { get; init; }
    public int Closed { get; init; }
}

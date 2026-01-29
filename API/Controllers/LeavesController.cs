using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Application.Interfaces;
using EmployeeInvestigationSystem.Domain.Entities;
using EmployeeInvestigationSystem.Domain.Enums;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeInvestigationSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeavesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;
    private readonly ILogger<LeavesController> _logger;

    public LeavesController(AppDbContext context, IAuditLogService auditLogService, IMapper mapper, ILogger<LeavesController> logger)
    {
        _context = context;
        _auditLogService = auditLogService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Create a new leave request (Admin only). Use submit=true to move directly to Submitted.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LeaveRequestDto>> Create(
        [FromBody] LeaveRequestCreateDto dto,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCreate(dto);
        if (validationError != null)
            return BadRequest(new { message = validationError });

        var leave = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = dto.EmployeeId.Trim(),
            EmployeeName = dto.EmployeeName.Trim(),
            Type = dto.Type,
            Status = dto.Submit ? LeaveStatus.Submitted : LeaveStatus.Draft,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = GetCurrentUserId(),
            CreatedByName = GetCurrentUserName()
        };

        if (dto.Attachments != null)
        {
            foreach (var attachment in dto.Attachments)
            {
                leave.Attachments.Add(new LeaveAttachment
                {
                    Id = Guid.NewGuid(),
                    LeaveRequestId = leave.Id,
                    FileId = attachment.FileId,
                    FileName = attachment.FileName,
                    SizeBytes = attachment.SizeBytes,
                    Url = attachment.Url,
                    UploadedAt = DateTime.UtcNow,
                    UploadedBy = leave.CreatedBy
                });
            }
        }

        if (RequiresAttachment(leave.Type) && leave.Attachments.Count == 0)
            return BadRequest(new { message = "Attachments are required for sick or Outside KSA leave types." });

        _context.LeaveRequests.Add(leave);
        await _context.SaveChangesAsync(cancellationToken);

        await LogAuditAsync("Create", leave, cancellationToken);
        var result = _mapper.Map<LeaveRequestDto>(leave);
        return CreatedAtAction(nameof(GetById), new { id = leave.Id }, result);
    }

    /// <summary>
    /// List leave requests with paging and filters.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,ER,Management")]
    public async Task<ActionResult<PagedResponse<LeaveRequestDto>>> GetLeaves(
        [FromQuery] string? employeeId,
        [FromQuery] int? type,
        [FromQuery] int? status,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 100);

        var query = _context.LeaveRequests
            .AsNoTracking()
            .Include(l => l.Attachments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(employeeId))
        {
            var normalized = employeeId.Trim().ToLowerInvariant();
            query = query.Where(l => l.EmployeeId.ToLower().Contains(normalized) || l.EmployeeName.ToLower().Contains(normalized));
        }

        if (type.HasValue)
        {
            query = query.Where(l => l.Type == (LeaveType)type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == (LeaveStatus)status.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(l => l.StartDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(l => l.EndDate <= to.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        var data = _mapper.Map<IEnumerable<LeaveRequestDto>>(items);

        return Ok(new PagedResponse<LeaveRequestDto>
        {
            Data = data,
            Page = page,
            Size = size,
            Total = total
        });
    }

    /// <summary>
    /// Get leave request detail.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,ER,Management")]
    public async Task<ActionResult<LeaveRequestDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var leave = await _context.LeaveRequests
            .AsNoTracking()
            .Include(l => l.Attachments)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (leave == null)
            return NotFound(new { message = "Leave request not found." });

        return Ok(_mapper.Map<LeaveRequestDto>(leave));
    }

    /// <summary>
    /// Submit a draft leave (Admin only).
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LeaveRequestDto>> Submit(Guid id, CancellationToken cancellationToken)
    {
        var leave = await _context.LeaveRequests
            .Include(l => l.Attachments)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (leave == null)
            return NotFound(new { message = "Leave request not found." });

        if (leave.Status != LeaveStatus.Draft)
            return Conflict(new { message = "Only draft leaves can be submitted." });

        if (RequiresAttachment(leave.Type) && leave.Attachments.Count == 0)
            return BadRequest(new { message = "Attachments are required for sick or Outside KSA leave types." });

        leave.Status = LeaveStatus.Submitted;
        await _context.SaveChangesAsync(cancellationToken);
        await LogAuditAsync("Submit", leave, cancellationToken);

        return Ok(_mapper.Map<LeaveRequestDto>(leave));
    }

    /// <summary>
    /// Review a leave request (ER only): StartReview, Approve, or Reject.
    /// </summary>
    [HttpPatch("{id:guid}/review")]
    [Authorize(Roles = "ER")]
    public async Task<ActionResult<LeaveRequestDto>> Review(
        Guid id,
        [FromBody] LeaveReviewDto dto,
        CancellationToken cancellationToken)
    {
        var leave = await _context.LeaveRequests
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (leave == null)
            return NotFound(new { message = "Leave request not found." });

        if (leave.Status is LeaveStatus.Approved or LeaveStatus.Rejected or LeaveStatus.Cancelled)
            return Conflict(new { message = "Leave is already finalized." });

        var decision = dto.Decision?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(decision))
            return BadRequest(new { message = "Decision is required." });

        var now = DateTime.UtcNow;
        var reviewerId = GetCurrentUserId();
        var reviewerName = GetCurrentUserName();

        switch (decision)
        {
            case "startreview":
                if (leave.Status != LeaveStatus.Submitted)
                    return Conflict(new { message = "Only submitted leaves can move to under review." });

                leave.Status = LeaveStatus.UnderReview;
                leave.ReviewedAt = now;
                leave.ReviewedBy = reviewerId;
                leave.ReviewedByName = reviewerName;
                leave.ReviewRemark = dto.Remark;
                break;

            case "approve":
            case "reject":
                if (leave.Status == LeaveStatus.Submitted)
                {
                    leave.Status = LeaveStatus.UnderReview;
                }
                else if (leave.Status != LeaveStatus.UnderReview)
                {
                    return Conflict(new { message = "Leave must be submitted or under review before a decision." });
                }

                leave.Status = decision == "approve" ? LeaveStatus.Approved : LeaveStatus.Rejected;
                leave.ReviewedAt = now;
                leave.ReviewedBy = reviewerId;
                leave.ReviewedByName = reviewerName;
                leave.ReviewRemark = dto.Remark;
                break;

            default:
                return BadRequest(new { message = "Decision must be StartReview, Approve, or Reject." });
        }

        await _context.SaveChangesAsync(cancellationToken);
        await LogAuditAsync(decision, leave, cancellationToken);

        return Ok(_mapper.Map<LeaveRequestDto>(leave));
    }

    private static bool RequiresAttachment(LeaveType type) =>
        type is LeaveType.Sick or LeaveType.OutsideKSA;

    private static string? ValidateCreate(LeaveRequestCreateDto dto)
    {
        if (dto.StartDate > dto.EndDate)
            return "Start date cannot be after end date.";

        if (string.IsNullOrWhiteSpace(dto.EmployeeId) || string.IsNullOrWhiteSpace(dto.EmployeeName))
            return "Employee details are required.";

        if (RequiresAttachment(dto.Type) && (dto.Attachments == null || dto.Attachments.Count == 0))
            return "Attachments are required for sick or Outside KSA leave types.";

        return null;
    }

    private async Task LogAuditAsync(string action, LeaveRequest leave, CancellationToken cancellationToken)
    {
        try
        {
            var audit = new AuditLogDto
            {
                UserId = GetCurrentUserId(),
                UserName = GetCurrentUserName(),
                EntityType = "LeaveRequest",
                EntityId = leave.Id.ToString(),
                Action = action,
                Endpoint = HttpContext.Request.Path,
                HttpMethod = HttpContext.Request.Method,
                StatusCode = 200,
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                NewValues = JsonSerializer.Serialize(new
                {
                    leave.EmployeeId,
                    leave.EmployeeName,
                    leave.Type,
                    leave.Status,
                    leave.StartDate,
                    leave.EndDate,
                    leave.ReviewRemark
                })
            };

            await _auditLogService.LogActionAsync(audit, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log audit for leave {LeaveId}", leave.Id);
        }
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    private string GetCurrentUserName()
    {
        return User.Identity?.Name ?? User.FindFirst(ClaimTypes.Email)?.Value ?? "System";
    }
}

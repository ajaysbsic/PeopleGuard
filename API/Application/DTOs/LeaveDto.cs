using EmployeeInvestigationSystem.Domain.Enums;

namespace EmployeeInvestigationSystem.Application.DTOs;

public record LeaveRequestCreateDto
{
    public required string EmployeeId { get; init; }
    public required string EmployeeName { get; init; }
    public required LeaveType Type { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public string? Reason { get; init; }
    public IReadOnlyList<FileAttachmentDto>? Attachments { get; init; }
    public bool Submit { get; init; } = true;
}

public record LeaveRequestDto
{
    public Guid LeaveId { get; init; }
    public string EmployeeId { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public LeaveType Type { get; init; }
    public LeaveStatus Status { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public string? Reason { get; init; }
    public IReadOnlyList<FileAttachmentDto> Attachments { get; init; } = Array.Empty<FileAttachmentDto>();
    public DateTime CreatedAt { get; init; }
    public Guid CreatedBy { get; init; }
    public string CreatedByName { get; init; } = string.Empty;
    public DateTime? ReviewedAt { get; init; }
    public Guid? ReviewedBy { get; init; }
    public string? ReviewedByName { get; init; }
    public string? ReviewRemark { get; init; }
}

public record LeaveFilterRequest
{
    public string? EmployeeId { get; init; }
    public LeaveType? Type { get; init; }
    public LeaveStatus? Status { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;
}

public record LeaveReviewDto
{
    public required string Decision { get; init; }
    public string? Remark { get; init; }
}

public record FileAttachmentDto
{
    public required string FileId { get; init; }
    public required string FileName { get; init; }
    public required long SizeBytes { get; init; }
    public required string Url { get; init; }
}

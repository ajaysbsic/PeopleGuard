using EmployeeInvestigationSystem.Domain.Enums;

namespace EmployeeInvestigationSystem.Domain.Entities;

public class LeaveRequest
{
    public Guid Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public LeaveType Type { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Draft;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public string? ReviewedByName { get; set; }
    public string? ReviewRemark { get; set; }

    public ICollection<LeaveAttachment> Attachments { get; set; } = new List<LeaveAttachment>();
}

public class LeaveAttachment
{
    public Guid Id { get; set; }
    public Guid LeaveRequestId { get; set; }
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public Guid? UploadedBy { get; set; }

    public LeaveRequest? LeaveRequest { get; set; }
}

using EmployeeInvestigationSystem.Domain.Enums;

namespace EmployeeInvestigationSystem.Application.DTOs;

public record CreateInvestigationDto
{
    public Guid EmployeeId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public InvestigationCaseType CaseType { get; init; }
}

public record UpdateInvestigationDto
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public InvestigationStatus Status { get; init; }
}

public record ChangeInvestigationStatusDto
{
    public InvestigationStatus Status { get; init; }
}

public record InvestigationDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public InvestigationCaseType CaseType { get; init; }
    public InvestigationStatus Status { get; init; }
    public WarningOutcome? Outcome { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
}

public record InvestigationRemarkDto
{
    public Guid Id { get; init; }
    public Guid InvestigationId { get; init; }
    public Guid UserId { get; init; }
    public string Remark { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record CreateInvestigationRemarkDto
{
    public Guid InvestigationId { get; init; }
    public string Remark { get; init; } = string.Empty;
}

public record InvestigationAttachmentDto
{
    public Guid Id { get; init; }
    public Guid InvestigationId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public DateTime UploadedAt { get; init; }
}

public record WarningLetterDto
{
    public Guid Id { get; init; }
    public Guid InvestigationId { get; init; }
    public Guid EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public WarningOutcome Outcome { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string PdfPath { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime IssuedAt { get; init; }
}

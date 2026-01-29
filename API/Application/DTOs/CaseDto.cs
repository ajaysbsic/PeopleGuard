namespace EmployeeInvestigationSystem.Application.DTOs;

/// <summary>
/// Generic paginated response wrapper
/// </summary>
public record PagedResponse<T>
{
    public IEnumerable<T> Data { get; init; } = Enumerable.Empty<T>();
    public int Page { get; init; }
    public int Size { get; init; }
    public int Total { get; init; }
    public int TotalPages => Size > 0 ? (int)Math.Ceiling((double)Total / Size) : 0;
}

/// <summary>
/// Case list item DTO with employee info for grid display
/// </summary>
public record CaseListItemDto
{
    public Guid Id { get; init; }
    public string CaseId { get; init; } = string.Empty; // Formatted case number like "C-2026-0001"
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string EmployeeCode { get; init; } = string.Empty; // Original employee ID like "EMP-001"
    public string Factory { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public int CaseType { get; init; }
    public string CaseTypeName { get; init; } = string.Empty;
    public int Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Full case detail DTO for detail view
/// </summary>
public record CaseDetailDto
{
    public Guid Id { get; init; }
    public string CaseId { get; init; } = string.Empty;
    
    // Employee info
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string EmployeeCode { get; init; } = string.Empty;
    public string Factory { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
    
    // Case info
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int CaseType { get; init; }
    public string CaseTypeName { get; init; } = string.Empty;
    public int Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public int? Outcome { get; init; }
    public string? OutcomeName { get; init; }
    
    // Timestamps
    public DateTime CreatedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    
    // Related counts
    public int RemarksCount { get; init; }
    public int AttachmentsCount { get; init; }
    public bool HasWarningLetter { get; init; }
    public Guid? WarningLetterId { get; init; }
}

/// <summary>
/// Case history/timeline event DTO
/// </summary>
public record CaseHistoryDto
{
    public Guid Id { get; init; }
    public int EventType { get; init; }
    public string EventTypeName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public Guid? ReferenceId { get; init; }
    public Guid? UserId { get; init; }
    public string? UserName { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Case remark DTO
/// </summary>
public record CaseRemarkDto
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Case attachment DTO
/// </summary>
public record CaseAttachmentDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public DateTime UploadedAt { get; init; }
    public string DownloadUrl { get; init; } = string.Empty;
}

/// <summary>
/// Case warning letter DTO
/// </summary>
public record CaseLetterDto
{
    public Guid Id { get; init; }
    public string Template { get; init; } = string.Empty;
    public int Outcome { get; init; }
    public string PdfUrl { get; init; } = string.Empty;
    public DateTime IssuedAt { get; init; }
}

/// <summary>
/// Request to update case status
/// </summary>
public record UpdateCaseStatusRequest
{
    public string Status { get; init; } = string.Empty; // "Open", "UnderInvestigation", "Closed"
    public int? Outcome { get; init; } // Required when closing
}

/// <summary>
/// Request to add a remark
/// </summary>
public record AddRemarkRequest
{
    public string Text { get; init; } = string.Empty;
}

/// <summary>
/// Request to set outcome/final decision
/// </summary>
public record SetOutcomeRequest
{
    public string Outcome { get; init; } = string.Empty; // None, VerbalWarning, WrittenWarning
    public string? FinalNote { get; init; }
}

/// <summary>
/// Request to generate warning letter
/// </summary>
public record GenerateLetterRequest
{
    public string Template { get; init; } = "standard"; // standard | manual
    public string? Html { get; init; }
}

/// <summary>
/// Query parameters for case list filtering
/// </summary>
public record CaseFilterParams
{
    public string? EmployeeId { get; init; }
    public string? Factory { get; init; }
    public int? CaseType { get; init; }
    public int? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;
    public string? SortBy { get; init; }
    public bool SortDesc { get; init; } = false;
}

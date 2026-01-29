using EmployeeInvestigationSystem.Domain.Enums;

namespace EmployeeInvestigationSystem.Domain.Entities;

public class Investigation
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InvestigationCaseType CaseType { get; set; }
    public InvestigationStatus Status { get; set; } = InvestigationStatus.Open;
    public WarningOutcome? Outcome { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    public Employee? Employee { get; set; }
    public ICollection<InvestigationRemark> Remarks { get; set; } = new List<InvestigationRemark>();
    public ICollection<InvestigationAttachment> Attachments { get; set; } = new List<InvestigationAttachment>();
    public ICollection<CaseHistory> History { get; set; } = new List<CaseHistory>();
    public ICollection<WarningLetter> WarningLetters { get; set; } = new List<WarningLetter>();
}

public class InvestigationRemark
{
    public Guid Id { get; set; }
    public Guid InvestigationId { get; set; }
    public Guid UserId { get; set; }
    public string Remark { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Investigation? Investigation { get; set; }
    public ApplicationUser? User { get; set; }
}

public class InvestigationAttachment
{
    public Guid Id { get; set; }
    public Guid InvestigationId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Investigation? Investigation { get; set; }
}

/// <summary>
/// Tracks all history events for a case (status changes, remarks, attachments, letters)
/// </summary>
public class CaseHistory
{
    public Guid Id { get; set; }
    public Guid InvestigationId { get; set; }
    public Guid? UserId { get; set; }
    public CaseHistoryEventType EventType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public Guid? ReferenceId { get; set; } // Links to RemarkId, AttachmentId, or WarningLetterId
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Investigation? Investigation { get; set; }
    public ApplicationUser? User { get; set; }
}

/// <summary>
/// Public submission captured via QR/Barcode entry.
/// </summary>
public class PublicSubmission
{
    public Guid Id { get; set; }
    public Guid QrTokenId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? AttachmentsJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public QrToken? QrToken { get; set; }
}

public enum CaseHistoryEventType
{
    Created = 1,
    StatusChanged = 2,
    RemarkAdded = 3,
    AttachmentAdded = 4,
    AttachmentRemoved = 5,
    WarningLetterIssued = 6,
    OutcomeSet = 7
}

public class WarningLetter
{
    public Guid Id { get; set; }
    public Guid InvestigationId { get; set; }
    public Guid EmployeeId { get; set; }
    public WarningOutcome Outcome { get; set; }
    public string Template { get; set; } = "standard";
    public string LetterContent { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string PdfPath { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public Investigation? Investigation { get; set; }
    public Employee? Employee { get; set; }
}

namespace EmployeeInvestigationSystem.Domain.Entities;

/// <summary>
/// QR token for public submissions (complaints, suggestions, etc.)
/// </summary>
public class QrToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty; // "employee" | "location" | "general"
    public string TargetId { get; set; } = string.Empty; // employee/location ID, or "general"
    public string? Label { get; set; } // display label (e.g., "Reception Desk", "HR Department")
    public string QrPngPath { get; set; } = string.Empty; // path to stored PNG
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } // default +30 days
    public bool IsActive { get; set; } = true;
    public Guid CreatedBy { get; set; }

    public ApplicationUser? Creator { get; set; }
}

/// <summary>
/// Submission received via QR token (creates case)
/// </summary>
public class QrSubmission
{
    public Guid Id { get; set; }
    public Guid TokenId { get; set; }
    public string Category { get; set; } = string.Empty; // "complaint", "suggestion", "safety_concern"
    public string Message { get; set; } = string.Empty;
    public string? SubmitterName { get; set; }
    public string? SubmitterEmail { get; set; }
    public string? SubmitterPhone { get; set; }
    public string? AttachmentUrls { get; set; } // comma-separated URLs or file paths
    public Guid? RelatedInvestigationId { get; set; } // case created from this
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public QrToken? Token { get; set; }
    public Investigation? Investigation { get; set; }
}

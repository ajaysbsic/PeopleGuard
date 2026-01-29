namespace EmployeeInvestigationSystem.Application.DTOs;

/// <summary>
/// Request to generate a new QR token for submissions
/// </summary>
public class GenerateQrTokenRequest
{
    public string TargetType { get; set; } = "general"; // "employee" | "location" | "general"
    public string TargetId { get; set; } = "general";
    public string? Label { get; set; }
}

/// <summary>
/// Response with QR token and image URL
/// </summary>
public class QrTokenResponse
{
    public Guid TokenId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string QrImageUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Request to submit a complaint/suggestion via QR
/// </summary>
public class QrSubmissionRequest
{
    public string Token { get; set; } = string.Empty;
    public string Category { get; set; } = "complaint"; // "complaint" | "suggestion" | "safety_concern"
    public string Message { get; set; } = string.Empty;
    public string? SubmitterName { get; set; }
    public string? SubmitterEmail { get; set; }
    public string? SubmitterPhone { get; set; }
    public List<string>? AttachmentUrls { get; set; } // URLs or file paths
}

/// <summary>
/// Response after successful submission
/// </summary>
public class QrSubmissionResponse
{
    public Guid SubmissionId { get; set; }
    public Guid? CaseId { get; set; }
    public string Message { get; set; } = "Thank you for your submission!";
}

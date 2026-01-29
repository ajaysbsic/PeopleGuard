namespace EmployeeInvestigationSystem.Application.DTOs;

/// <summary>
/// DTO for employee history view (all investigations and warnings).
/// </summary>
public class EmployeeHistoryDto
{
    /// <summary>
    /// Employee ID.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Employee name.
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Employee ID number.
    /// </summary>
    public string EmployeeIdNumber { get; set; } = string.Empty;

    /// <summary>
    /// Department.
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Factory.
    /// </summary>
    public string Factory { get; set; } = string.Empty;

    /// <summary>
    /// Total investigations count.
    /// </summary>
    public int TotalInvestigations { get; set; }

    /// <summary>
    /// Total warnings count.
    /// </summary>
    public int TotalWarnings { get; set; }

    /// <summary>
    /// Verbal warnings count.
    /// </summary>
    public int VerbalWarnings { get; set; }

    /// <summary>
    /// Written warnings count.
    /// </summary>
    public int WrittenWarnings { get; set; }

    /// <summary>
    /// Total no-action cases.
    /// </summary>
    public int NoActionCases { get; set; }

    /// <summary>
    /// Last investigation date.
    /// </summary>
    public DateTime? LastInvestigationDate { get; set; }

    /// <summary>
    /// Last warning date.
    /// </summary>
    public DateTime? LastWarningDate { get; set; }

    /// <summary>
    /// Investigation history records.
    /// </summary>
    public IEnumerable<InvestigationHistoryItemDto> InvestigationHistory { get; set; } = new List<InvestigationHistoryItemDto>();

    /// <summary>
    /// Warning history records.
    /// </summary>
    public IEnumerable<WarningHistoryItemDto> WarningHistory { get; set; } = new List<WarningHistoryItemDto>();
}

/// <summary>
/// Single investigation history item.
/// </summary>
public class InvestigationHistoryItemDto
{
    /// <summary>
    /// Investigation ID.
    /// </summary>
    public Guid InvestigationId { get; set; }

    /// <summary>
    /// Investigation title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Investigation description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Case type.
    /// </summary>
    public string CaseType { get; set; } = string.Empty;

    /// <summary>
    /// Investigation status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Investigation outcome (if closed).
    /// </summary>
    public string? Outcome { get; set; }

    /// <summary>
    /// Date created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date closed.
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// Remarks count.
    /// </summary>
    public int RemarksCount { get; set; }

    /// <summary>
    /// Attachments count.
    /// </summary>
    public int AttachmentsCount { get; set; }
}

/// <summary>
/// Single warning history item.
/// </summary>
public class WarningHistoryItemDto
{
    /// <summary>
    /// Warning letter ID.
    /// </summary>
    public Guid WarningLetterId { get; set; }

    /// <summary>
    /// Related investigation ID.
    /// </summary>
    public Guid InvestigationId { get; set; }

    /// <summary>
    /// Warning outcome type.
    /// </summary>
    public string Outcome { get; set; } = string.Empty;

    /// <summary>
    /// Date issued.
    /// </summary>
    public DateTime IssuedAt { get; set; }

    /// <summary>
    /// Reason for warning.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Flattened employee history row for profile table (investigations + warnings).
/// </summary>
public class EmployeeHistoryListItemDto
{
    public Guid Id { get; set; }
    public Guid? InvestigationId { get; set; }
    public string Kind { get; set; } = string.Empty; // Investigation | Warning
    public string Title { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Outcome { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Aggregated employee stats for profile header tiles.
/// </summary>
public class EmployeeStatsDto
{
    public int TotalCases { get; set; }
    public int Open { get; set; }
    public int Closed { get; set; }
    public int VerbalWarnings { get; set; }
    public int WrittenWarnings { get; set; }
}

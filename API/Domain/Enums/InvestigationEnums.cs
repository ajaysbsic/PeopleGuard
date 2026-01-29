namespace EmployeeInvestigationSystem.Domain.Enums;

public enum InvestigationStatus
{
    Open = 1,
    UnderInvestigation = 2,
    Closed = 3
}

public enum InvestigationCaseType
{
    Violation = 1,
    Safety = 2,
    Misbehavior = 3,
    Investigation = 4,
    Complaint = 5
}

public enum WarningOutcome
{
    NoAction = 1,
    VerbalWarning = 2,
    WrittenWarning = 3
}

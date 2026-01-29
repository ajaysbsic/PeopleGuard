namespace EmployeeInvestigationSystem.Domain.Enums;

public enum LeaveType
{
    Emergency = 1,
    Sick = 2,
    OutsideKSA = 3
}

public enum LeaveStatus
{
    Draft = 1,
    Submitted = 2,
    UnderReview = 3,
    Approved = 4,
    Rejected = 5,
    Cancelled = 6
}

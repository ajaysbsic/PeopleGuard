using EmployeeInvestigationSystem.Domain.Enums;

namespace EmployeeInvestigationSystem.Application.DTOs;

public record CreateEmployeeDto
{
    public string EmployeeId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Factory { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
}

public record UpdateEmployeeDto
{
    public string Name { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Factory { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
    public EmployeeStatus Status { get; init; } = EmployeeStatus.Active;
}

public record EmployeeDto
{
    public Guid Id { get; init; }
    public string EmployeeId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Factory { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
    public EmployeeStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

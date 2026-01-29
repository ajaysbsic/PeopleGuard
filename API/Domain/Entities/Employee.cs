using EmployeeInvestigationSystem.Domain.Enums;

namespace EmployeeInvestigationSystem.Domain.Entities;

public class Employee
{
    public Guid Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Factory { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}

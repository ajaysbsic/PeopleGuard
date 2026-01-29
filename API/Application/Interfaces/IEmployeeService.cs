using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Domain.Entities;

namespace EmployeeInvestigationSystem.Application.Interfaces;

public interface IEmployeeService
{
    Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmployeeDto?> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EmployeeDto>> SearchAsync(string? name, string? department, string? factory, CancellationToken cancellationToken = default);
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default);
    Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id);
    Task<Employee?> GetByEmployeeIdAsync(string employeeId);
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<IEnumerable<Employee>> SearchAsync(string? name, string? department, string? factory);
    Task AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}

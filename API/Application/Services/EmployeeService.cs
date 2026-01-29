using AutoMapper;
using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Application.Interfaces;
using EmployeeInvestigationSystem.Domain.Entities;

namespace EmployeeInvestigationSystem.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IMapper _mapper;

    public EmployeeService(IEmployeeRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id);
        return employee == null ? null : _mapper.Map<EmployeeDto>(employee);
    }

    public async Task<EmployeeDto?> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByEmployeeIdAsync(employeeId);
        return employee == null ? null : _mapper.Map<EmployeeDto>(employee);
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<IEnumerable<EmployeeDto>> SearchAsync(string? name, string? department, string? factory, CancellationToken cancellationToken = default)
    {
        var employees = await _repository.SearchAsync(name, department, factory);
        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var employee = _mapper.Map<Employee>(dto);
        await _repository.AddAsync(employee);
        await _repository.SaveChangesAsync();
        return _mapper.Map<EmployeeDto>(employee);
    }

    public async Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {id} not found.");
        }

        _mapper.Map(dto, employee);
        employee.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(employee);
        await _repository.SaveChangesAsync();
        return _mapper.Map<EmployeeDto>(employee);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {id} not found.");
        }

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();
    }
}

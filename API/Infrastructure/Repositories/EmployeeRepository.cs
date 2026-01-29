using EmployeeInvestigationSystem.Application.Interfaces;
using EmployeeInvestigationSystem.Domain.Entities;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace EmployeeInvestigationSystem.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(Guid id)
    {
        return await _context.Set<Employee>().Where(e => !e.IsDeleted).FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Employee?> GetByEmployeeIdAsync(string employeeId)
    {
        return await _context.Set<Employee>().Where(e => !e.IsDeleted).FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _context.Set<Employee>().Where(e => !e.IsDeleted).ToListAsync();
    }

    public async Task<IEnumerable<Employee>> SearchAsync(string? name, string? department, string? factory)
    {
        var query = _context.Set<Employee>().Where(e => !e.IsDeleted);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(e => e.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(e => e.Department.Contains(department));

        if (!string.IsNullOrWhiteSpace(factory))
            query = query.Where(e => e.Factory.Contains(factory));

        return await query.ToListAsync();
    }

    public async Task AddAsync(Employee employee)
    {
        await _context.Set<Employee>().AddAsync(employee);
    }

    public async Task UpdateAsync(Employee employee)
    {
        _context.Set<Employee>().Update(employee);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var employee = await GetByIdAsync(id);
        if (employee != null)
        {
            employee.IsDeleted = true;
            _context.Set<Employee>().Update(employee);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

using EmployeeInvestigationSystem.Application.Interfaces;
using EmployeeInvestigationSystem.Domain.Entities;
using EmployeeInvestigationSystem.Domain.Enums;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace EmployeeInvestigationSystem.Infrastructure.Repositories;

public class InvestigationRepository : IInvestigationRepository
{
    private readonly AppDbContext _context;

    public InvestigationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Investigation?> GetByIdAsync(Guid id)
    {
        return await _context.Investigations
            .Include(i => i.Employee)
            .Include(i => i.Remarks)
            .Include(i => i.Attachments)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
    }

    public async Task<IEnumerable<Investigation>> GetByEmployeeIdAsync(Guid employeeId)
    {
        return await _context.Investigations
            .Where(i => i.EmployeeId == employeeId && !i.IsDeleted)
            .Include(i => i.Employee)
            .ToListAsync();
    }

    public async Task<IEnumerable<Investigation>> GetAllAsync()
    {
        return await _context.Investigations
            .Where(i => !i.IsDeleted)
            .Include(i => i.Employee)
            .ToListAsync();
    }

    public async Task<IEnumerable<Investigation>> GetByStatusAsync(InvestigationStatus status)
    {
        return await _context.Investigations
            .Where(i => i.Status == status && !i.IsDeleted)
            .Include(i => i.Employee)
            .ToListAsync();
    }

    public async Task AddAsync(Investigation investigation)
    {
        await _context.Investigations.AddAsync(investigation);
    }

    public async Task UpdateAsync(Investigation investigation)
    {
        _context.Investigations.Update(investigation);
        await Task.CompletedTask;
    }

    public async Task AddRemarkAsync(InvestigationRemark remark)
    {
        await _context.Set<InvestigationRemark>().AddAsync(remark);
    }

    public async Task<IEnumerable<InvestigationRemark>> GetRemarksAsync(Guid investigationId)
    {
        return await _context.Set<InvestigationRemark>()
            .Where(r => r.InvestigationId == investigationId)
            .Include(r => r.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<InvestigationAttachment>> GetAttachmentsAsync(Guid investigationId)
    {
        return await _context.Set<InvestigationAttachment>()
            .Where(a => a.InvestigationId == investigationId)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

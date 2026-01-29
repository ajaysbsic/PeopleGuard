using AutoMapper;
using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Application.Interfaces;
using EmployeeInvestigationSystem.Domain.Entities;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace EmployeeInvestigationSystem.Infrastructure.Services;

/// <summary>
/// Service for audit logging operations.
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public AuditLogService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Guid> LogActionAsync(AuditLogDto auditLog, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<AuditLog>(auditLog);
        entity.Id = Guid.NewGuid();
        entity.Timestamp = DateTime.UtcNow;

        _context.AuditLogs.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsForEntityAsync(string entityId, CancellationToken cancellationToken = default)
    {
        var logs = await _context.AuditLogs
            .Where(a => a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
    }

    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var logs = await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
    }

    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? action = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(a => a.Action == action);
        }

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
    }

    public async Task<int> DeleteOldAuditLogsAsync(int retentionDays = 90, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        var logsToDelete = await _context.AuditLogs
            .Where(a => a.Timestamp < cutoffDate)
            .ToListAsync(cancellationToken);

        _context.AuditLogs.RemoveRange(logsToDelete);
        await _context.SaveChangesAsync(cancellationToken);

        return logsToDelete.Count;
    }
}

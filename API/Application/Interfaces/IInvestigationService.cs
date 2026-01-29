using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Domain.Entities;
using EmployeeInvestigationSystem.Domain.Enums;

namespace EmployeeInvestigationSystem.Application.Interfaces;

public interface IInvestigationService
{
    Task<InvestigationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<InvestigationDto>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InvestigationDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<InvestigationDto>> GetByStatusAsync(InvestigationStatus status, CancellationToken cancellationToken = default);
    Task<InvestigationDto> CreateAsync(CreateInvestigationDto dto, CancellationToken cancellationToken = default);
    Task<InvestigationDto> UpdateAsync(Guid id, UpdateInvestigationDto dto, CancellationToken cancellationToken = default);
    Task<InvestigationDto> ChangeStatusAsync(Guid id, InvestigationStatus status, CancellationToken cancellationToken = default);
    Task AddRemarkAsync(Guid investigationId, CreateInvestigationRemarkDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InvestigationRemarkDto>> GetRemarksAsync(Guid investigationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InvestigationAttachmentDto>> GetAttachmentsAsync(Guid investigationId, CancellationToken cancellationToken = default);
}

public interface IInvestigationRepository
{
    Task<Investigation?> GetByIdAsync(Guid id);
    Task<IEnumerable<Investigation>> GetByEmployeeIdAsync(Guid employeeId);
    Task<IEnumerable<Investigation>> GetAllAsync();
    Task<IEnumerable<Investigation>> GetByStatusAsync(InvestigationStatus status);
    Task AddAsync(Investigation investigation);
    Task UpdateAsync(Investigation investigation);
    Task AddRemarkAsync(InvestigationRemark remark);
    Task<IEnumerable<InvestigationRemark>> GetRemarksAsync(Guid investigationId);
    Task<IEnumerable<InvestigationAttachment>> GetAttachmentsAsync(Guid investigationId);
    Task SaveChangesAsync();
}

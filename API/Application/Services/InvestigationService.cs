using AutoMapper;
using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Application.Interfaces;
using EmployeeInvestigationSystem.Domain.Entities;
using EmployeeInvestigationSystem.Domain.Enums;

namespace EmployeeInvestigationSystem.Application.Services;

public class InvestigationService : IInvestigationService
{
    private readonly IInvestigationRepository _repository;
    private readonly IMapper _mapper;

    public InvestigationService(IInvestigationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<InvestigationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var investigation = await _repository.GetByIdAsync(id);
        return investigation == null ? null : _mapper.Map<InvestigationDto>(investigation);
    }

    public async Task<IEnumerable<InvestigationDto>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var investigations = await _repository.GetByEmployeeIdAsync(employeeId);
        return _mapper.Map<IEnumerable<InvestigationDto>>(investigations);
    }

    public async Task<IEnumerable<InvestigationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var investigations = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<InvestigationDto>>(investigations);
    }

    public async Task<IEnumerable<InvestigationDto>> GetByStatusAsync(InvestigationStatus status, CancellationToken cancellationToken = default)
    {
        var investigations = await _repository.GetByStatusAsync(status);
        return _mapper.Map<IEnumerable<InvestigationDto>>(investigations);
    }

    public async Task<InvestigationDto> CreateAsync(CreateInvestigationDto dto, CancellationToken cancellationToken = default)
    {
        var investigation = new Investigation
        {
            Id = Guid.NewGuid(),
            EmployeeId = dto.EmployeeId,
            Title = dto.Title,
            Description = dto.Description,
            CaseType = dto.CaseType,
            Status = InvestigationStatus.Open
        };

        await _repository.AddAsync(investigation);
        await _repository.SaveChangesAsync();
        return _mapper.Map<InvestigationDto>(investigation);
    }

    public async Task<InvestigationDto> UpdateAsync(Guid id, UpdateInvestigationDto dto, CancellationToken cancellationToken = default)
    {
        var investigation = await _repository.GetByIdAsync(id);
        if (investigation == null)
        {
            throw new KeyNotFoundException($"Investigation with ID {id} not found.");
        }

        // Prevent update if closed
        if (investigation.Status == InvestigationStatus.Closed)
        {
            throw new InvalidOperationException("Cannot update a closed investigation.");
        }

        investigation.Title = dto.Title;
        investigation.Description = dto.Description;
        await _repository.UpdateAsync(investigation);
        await _repository.SaveChangesAsync();
        return _mapper.Map<InvestigationDto>(investigation);
    }

    public async Task<InvestigationDto> ChangeStatusAsync(Guid id, InvestigationStatus status, CancellationToken cancellationToken = default)
    {
        var investigation = await _repository.GetByIdAsync(id);
        if (investigation == null)
        {
            throw new KeyNotFoundException($"Investigation with ID {id} not found.");
        }

        // Validate status transitions
        if (investigation.Status == InvestigationStatus.Closed)
        {
            throw new InvalidOperationException("Cannot change status of a closed investigation.");
        }

        investigation.Status = status;
        if (status == InvestigationStatus.Closed)
        {
            investigation.ClosedAt = DateTime.UtcNow;
        }

        await _repository.UpdateAsync(investigation);
        await _repository.SaveChangesAsync();
        return _mapper.Map<InvestigationDto>(investigation);
    }

    public async Task AddRemarkAsync(Guid investigationId, CreateInvestigationRemarkDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        var investigation = await _repository.GetByIdAsync(investigationId);
        if (investigation == null)
        {
            throw new KeyNotFoundException($"Investigation with ID {investigationId} not found.");
        }

        var remark = new InvestigationRemark
        {
            Id = Guid.NewGuid(),
            InvestigationId = investigationId,
            UserId = userId,
            Remark = dto.Remark
        };

        await _repository.AddRemarkAsync(remark);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<InvestigationRemarkDto>> GetRemarksAsync(Guid investigationId, CancellationToken cancellationToken = default)
    {
        var remarks = await _repository.GetRemarksAsync(investigationId);
        return _mapper.Map<IEnumerable<InvestigationRemarkDto>>(remarks);
    }

    public async Task<IEnumerable<InvestigationAttachmentDto>> GetAttachmentsAsync(Guid investigationId, CancellationToken cancellationToken = default)
    {
        var attachments = await _repository.GetAttachmentsAsync(investigationId);
        return _mapper.Map<IEnumerable<InvestigationAttachmentDto>>(attachments);
    }
}

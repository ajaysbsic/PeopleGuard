using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Domain.Enums;

namespace EmployeeInvestigationSystem.Application.Interfaces;

public interface IWarningLetterService
{
    Task<IEnumerable<WarningLetterDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WarningLetterDto> CreateWarningLetterAsync(Guid investigationId, WarningOutcome outcome, Guid employeeId, string reason, CancellationToken cancellationToken = default);
    Task<WarningLetterDto?> GetByInvestigationIdAsync(Guid investigationId, CancellationToken cancellationToken = default);
    Task<byte[]> GetWarningLetterPdfAsync(Guid warningLetterId, CancellationToken cancellationToken = default);
}

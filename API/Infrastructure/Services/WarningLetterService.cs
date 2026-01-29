using AutoMapper;
using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Application.Interfaces;
using EmployeeInvestigationSystem.Domain.Entities;
using EmployeeInvestigationSystem.Domain.Enums;
using EmployeeInvestigationSystem.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EmployeeInvestigationSystem.Infrastructure.Services;

public class WarningLetterService : IWarningLetterService
{
    private readonly AppDbContext _context;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPdfGenerationService _pdfGenerationService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public WarningLetterService(
        AppDbContext context,
        IEmployeeRepository employeeRepository,
        IPdfGenerationService pdfGenerationService,
        IMapper mapper,
        IConfiguration configuration)
    {
        _context = context;
        _employeeRepository = employeeRepository;
        _pdfGenerationService = pdfGenerationService;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<WarningLetterDto> CreateWarningLetterAsync(
        Guid investigationId,
        WarningOutcome outcome,
        Guid employeeId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var investigation = await _context.Investigations.FirstOrDefaultAsync(i => i.Id == investigationId && !i.IsDeleted);
        if (investigation == null)
        {
            throw new KeyNotFoundException($"Investigation with ID {investigationId} not found.");
        }

        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");
        }

        var outcomeName = outcome switch
        {
            WarningOutcome.NoAction => "No Action Required",
            WarningOutcome.VerbalWarning => "VERBAL WARNING",
            WarningOutcome.WrittenWarning => "WRITTEN WARNING",
            _ => "UNKNOWN"
        };

        var issuedAtLocal = DateTime.Now;
        var pdfBytes = await _pdfGenerationService.GenerateWarningLetterPdfAsync(
            employee.Name,
            employee.EmployeeId,
            employee.Department,
            outcomeName,
            reason,
            issuedAtLocal);

        var storageRoot = _configuration.GetValue<string>("WarningLetters:StoragePath");
        if (string.IsNullOrWhiteSpace(storageRoot))
        {
            storageRoot = Path.Combine(AppContext.BaseDirectory, "storage", "warnings");
        }
        Directory.CreateDirectory(storageRoot);

        var fileName = $"Warning Letter-{issuedAtLocal:ddMMyyyyHHmmss}.pdf";
        var fullPath = Path.Combine(storageRoot, fileName);
        await File.WriteAllBytesAsync(fullPath, pdfBytes, cancellationToken);
        
        // Save PDF (in production, this would be to blob storage or file system)
        var warningLetter = new WarningLetter
        {
            Id = Guid.NewGuid(),
            InvestigationId = investigationId,
            EmployeeId = employeeId,
            Outcome = outcome,
            LetterContent = reason,
            PdfPath = fullPath,
            IssuedAt = issuedAtLocal.ToUniversalTime()
        };

        investigation.Outcome = outcome;
        await _context.WarningLetters.AddAsync(warningLetter);
        _context.Investigations.Update(investigation);
        await _context.SaveChangesAsync();

        return _mapper.Map<WarningLetterDto>(warningLetter);
    }

    public async Task<IEnumerable<WarningLetterDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var letters = await _context.WarningLetters
            .Include(w => w.Employee)
            .OrderByDescending(w => w.IssuedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<WarningLetterDto>>(letters);
    }

    public async Task<WarningLetterDto?> GetByInvestigationIdAsync(Guid investigationId, CancellationToken cancellationToken = default)
    {
        var letter = await _context.WarningLetters
            .FirstOrDefaultAsync(w => w.InvestigationId == investigationId);

        return letter == null ? null : _mapper.Map<WarningLetterDto>(letter);
    }

    public async Task<byte[]> GetWarningLetterPdfAsync(Guid warningLetterId, CancellationToken cancellationToken = default)
    {
        var letter = await _context.WarningLetters.FirstOrDefaultAsync(w => w.Id == warningLetterId);
        if (letter == null)
        {
            throw new KeyNotFoundException($"Warning letter with ID {warningLetterId} not found.");
        }

        if (!File.Exists(letter.PdfPath))
        {
            throw new FileNotFoundException($"Stored PDF not found at {letter.PdfPath}");
        }
        return await File.ReadAllBytesAsync(letter.PdfPath, cancellationToken);
    }
}

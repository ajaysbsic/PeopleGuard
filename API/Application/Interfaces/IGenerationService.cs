namespace EmployeeInvestigationSystem.Application.Interfaces;

public interface IQRCodeService
{
    byte[] GenerateQRCode(string data, int pixelsPerModule = 20);
}

public interface IPdfGenerationService
{
    Task<byte[]> GenerateWarningLetterPdfAsync(string employeeName, string employeeId, string department, string outcome, string reason, DateTime issuedDate);
}

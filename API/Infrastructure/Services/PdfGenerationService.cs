using EmployeeInvestigationSystem.Application.Interfaces;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace EmployeeInvestigationSystem.Infrastructure.Services;

public class PdfGenerationService : IPdfGenerationService
{
    public async Task<byte[]> GenerateWarningLetterPdfAsync(
        string employeeName,
        string employeeId,
        string department,
        string outcome,
        string reason,
        DateTime issuedDate)
    {
        using (var memoryStream = new MemoryStream())
        {
            var pdfWriter = new PdfWriter(memoryStream);
            var pdfDocument = new PdfDocument(pdfWriter);
            var document = new Document(pdfDocument);

            // Header
            var header = new Paragraph("EMPLOYEE WARNING LETTER")
                .SetFontSize(18)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(header);

            // Company Info
            var companyInfo = new Paragraph("Employee Investigation & Violation Management System")
                .SetFontSize(11)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(30);
            document.Add(companyInfo);

            // Issue Date
            var issueInfo = new Paragraph($"Date of Issue: {issuedDate:dd MMMM yyyy}")
                .SetFontSize(10)
                .SetMarginBottom(20);
            document.Add(issueInfo);

            // Employee Details
            document.Add(new Paragraph("EMPLOYEE DETAILS")
                .SetBold()
                .SetFontSize(12)
                .SetMarginBottom(10));

            var detailsTable = new Table(2)
                .SetWidth(UnitValue.CreatePercentValue(100));
            
            detailsTable.AddCell(new Cell().Add(new Paragraph("Employee Name:").SetBold()));
            detailsTable.AddCell(new Cell().Add(new Paragraph(employeeName)));
            
            detailsTable.AddCell(new Cell().Add(new Paragraph("Employee ID:").SetBold()));
            detailsTable.AddCell(new Cell().Add(new Paragraph(employeeId)));
            
            detailsTable.AddCell(new Cell().Add(new Paragraph("Department:").SetBold()));
            detailsTable.AddCell(new Cell().Add(new Paragraph(department)));
            
            document.Add(detailsTable);
            document.Add(new Paragraph("").SetMarginBottom(15));

            // Warning Outcome
            document.Add(new Paragraph("WARNING OUTCOME")
                .SetBold()
                .SetFontSize(12)
                .SetMarginBottom(10));
            
            var outcomeTable = new Table(1)
                .SetWidth(UnitValue.CreatePercentValue(100));
            outcomeTable.AddCell(new Cell().Add(new Paragraph(outcome).SetBold().SetFontSize(14)));
            document.Add(outcomeTable);
            document.Add(new Paragraph("").SetMarginBottom(15));

            // Violation Details
            document.Add(new Paragraph("VIOLATION DETAILS")
                .SetBold()
                .SetFontSize(12)
                .SetMarginBottom(10));
            
            var reasonPara = new Paragraph(reason)
                .SetFontSize(11)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetMarginBottom(20);
            document.Add(reasonPara);

            // Footer
            document.Add(new Paragraph("").SetMarginBottom(30));
            document.Add(new Paragraph("This is an official warning. Further violations may result in disciplinary action.")
                .SetFontSize(10)
                .SetItalic()
                .SetTextAlignment(TextAlignment.CENTER));

            document.Close();
            return memoryStream.ToArray();
        }
    }
}

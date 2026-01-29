using QRCoder;
using EmployeeInvestigationSystem.Application.Interfaces;

namespace EmployeeInvestigationSystem.Infrastructure.Services;

public class QRCodeService : IQRCodeService
{
    public byte[] GenerateQRCode(string data, int pixelsPerModule = 20)
    {
        using (var qrGenerator = new QRCodeGenerator())
        {
            var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using (var qrCode = new PngByteQRCode(qrCodeData))
            {
                return qrCode.GetGraphic(pixelsPerModule);
            }
        }
    }
}

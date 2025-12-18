using CarRentalSystem.Application.Common.Interfaces;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalSystem.Infrastructure.Services
{
    public class QRCodeService : IQRCodeService
    {
        public string GenerateQRCode(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);

            var qrCodeBytes = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeBytes);
        }

        public string GenerateQRCodeAsFile(string content, string fileName)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);

            var qrCodeBytes = qrCode.GetGraphic(20);

            // Save to wwwroot/qrcodes directory
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "qrcodes");
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, fileName);
            File.WriteAllBytes(filePath, qrCodeBytes);

            return $"/qrcodes/{fileName}";
        }
    }
}

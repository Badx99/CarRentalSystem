using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalSystem.Application.Common.Interfaces
{
    public interface IQRCodeService
    {
        // Generates a QR code as Base64 string
        string GenerateQRCode(string content);

        // GENERATES QR CODE AS A FILE
        string GenerateQRCodeAsFile(string content,string fileName);
    }
}

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Models.QRCode;
using Refundeo.Core.Models.RefundCase;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IRefundCaseService
    {
        ObjectResult GenerateRefundCaseDtoResponse(IEnumerable<RefundCase> refundCases);
        ObjectResult GenerateRefundCaseDtoResponse(RefundCase refundCase);
        RefundCaseDto ConvertRefundCaseToDto(RefundCase refundCase);
        byte[] GenerateQrCode(int height, int width, int margin, QRCodePayloadDto payload);
        string ConvertByteArrayToBase64(byte[] ba);
        byte[] ConvertBase64ToByteArray(string base64String);
    }
}

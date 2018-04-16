using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Models.QRCode;
using Refundeo.Core.Models.RefundCase;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IRefundCaseService
    {
        ObjectResult GenerateRefundCaseDTOResponse(IEnumerable<RefundCase> refundCases);
        ObjectResult GenerateRefundCaseDTOResponse(RefundCase refundCase);
        RefundCaseDTO ConvertRefundCaseToDTO(RefundCase refundCase);
        byte[] GenerateQRCode(int height, int width, int margin, QRCodePayloadDTO payload);
        string ConvertByteArrayToBase64(byte[] ba);
        byte[] ConvertBase64ToByteArray(string base64String);
    }
}
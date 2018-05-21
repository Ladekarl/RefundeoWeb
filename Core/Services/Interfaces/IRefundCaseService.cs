using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Models.QRCode;
using Refundeo.Core.Models.RefundCase;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IRefundCaseService
    {
        Task<ObjectResult> GenerateRefundCaseDtoResponseAsync(IEnumerable<RefundCase> refundCases);
        Task<ObjectResult> GenerateRefundCaseDtoResponseAsync(RefundCase refundCase);
        Task<RefundCaseDto> ConvertRefundCaseToDtoAsync(RefundCase refundCase);
        byte[] GenerateQrCode(int height, int width, int margin, QRCodePayloadDto payload);
        string ConvertByteArrayToBase64(byte[] ba);
        byte[] ConvertBase64ToByteArray(string base64String);
    }
}

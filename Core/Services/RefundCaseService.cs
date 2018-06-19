using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Models.QRCode;
using Refundeo.Core.Models.RefundCase;
using Refundeo.Core.Services.Interfaces;
using ZXing;
using ZXing.QrCode;

namespace Refundeo.Core.Services
{
    public class RefundCaseService : IRefundCaseService
    {
        private readonly IUtilityService _utilityService;

        public RefundCaseService(IUtilityService utilityService)
        {
            _utilityService = utilityService;
        }

        public async Task<ObjectResult> GenerateRefundCaseDtoResponseAsync(IEnumerable<RefundCase> refundCases)
        {
            var dtos = new List<RefundCaseDto>();
            foreach (var refundCase in refundCases)
            {
                dtos.Add(await ConvertRefundCaseToDtoAsync(refundCase));
            }

            return new ObjectResult(dtos);
        }

        public async Task<ObjectResult> GenerateRefundCaseDtoResponseAsync(RefundCase refundCase)
        {
            var dto = await ConvertRefundCaseToDtoAsync(refundCase);
            return new ObjectResult(dto);
        }

        public async Task<RefundCaseDto> ConvertRefundCaseToDtoAsync(RefundCase refundCase)
        {
            return new RefundCaseDto
            {
                Id = refundCase.Id,
                Amount = refundCase.Amount,
                RefundAmount = refundCase.RefundAmount,
                IsRequested = refundCase.IsRequested,
                IsAccepted = refundCase.IsAccepted,
                IsRejected = refundCase.IsRejected,
                QrCode =  await _utilityService.ConvertBlobPathToBase64Async(refundCase.QRCode),
                VatFormImage = await _utilityService.ConvertBlobPathToBase64Async(refundCase.VATFormImage),
                ReceiptImage = await _utilityService.ConvertBlobPathToBase64Async(refundCase.ReceiptImage),
                DateCreated = refundCase.DateCreated,
                DateRequested = refundCase.DateRequested,
                ReceiptNumber = refundCase.ReceiptNumber,
                Customer = await _utilityService.ConvertCustomerInformationToDtoAsync(refundCase.CustomerInformation),
                Merchant = await _utilityService.ConvertMerchantInformationToDtoAsync(refundCase.MerchantInformation)
            };
        }
    }
}

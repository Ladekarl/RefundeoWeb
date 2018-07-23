using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Refundeo.Core.Data;
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
        private readonly IBlobStorageService _blobStorageService;
        private readonly RefundeoDbContext _context;

        public RefundCaseService(IUtilityService utilityService, IBlobStorageService blobStorageService,
            RefundeoDbContext context)
        {
            _utilityService = utilityService;
            _blobStorageService = blobStorageService;
            _context = context;
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
                VatAmount = refundCase.VATAmount,
                MerchantAmount = refundCase.MerchantAmount,
                AdminAmount = refundCase.AdminAmount,
                IsRequested = refundCase.IsRequested,
                IsAccepted = refundCase.IsAccepted,
                IsRejected = refundCase.IsRejected,
                QrCode = await _utilityService.ConvertBlobPathToBase64Async(refundCase.QRCode),
                VatFormImage = await _utilityService.ConvertBlobPathToBase64Async(refundCase.VATFormImage),
                ReceiptImage = await _utilityService.ConvertBlobPathToBase64Async(refundCase.ReceiptImage),
                DateCreated = refundCase.DateCreated,
                DateRequested = refundCase.DateRequested,
                ReceiptNumber = refundCase.ReceiptNumber,
                Customer = await _utilityService.ConvertCustomerInformationToDtoAsync(refundCase.CustomerInformation),
                Merchant = await _utilityService.ConvertMerchantInformationToDtoAsync(refundCase.MerchantInformation),
                CustomerSignature = await _utilityService.ConvertBlobPathToBase64Async(refundCase.CustomerSignature),
                MerchantSignature = await _utilityService.ConvertBlobPathToBase64Async(refundCase.MerchantSignature)
            };
        }

        public async Task DeleteRefundCasesAsync(ICollection<RefundCase> refundCases)
        {
            foreach (var refundCase in refundCases)
            {
                if (!string.IsNullOrEmpty(refundCase.QRCode))
                    await _blobStorageService.DeleteAsync(new Uri(refundCase.QRCode));
                if (!string.IsNullOrEmpty(refundCase.ReceiptImage))
                    await _blobStorageService.DeleteAsync(new Uri(refundCase.ReceiptImage));
                if (!string.IsNullOrEmpty(refundCase.VATFormImage))
                    await _blobStorageService.DeleteAsync(new Uri(refundCase.VATFormImage));
                if (!string.IsNullOrEmpty(refundCase.CustomerSignature))
                    await _blobStorageService.DeleteAsync(new Uri(refundCase.CustomerSignature));
                if (!string.IsNullOrEmpty(refundCase.MerchantSignature))
                    await _blobStorageService.DeleteAsync(new Uri(refundCase.MerchantSignature));
            }

            _context.RemoveRange(refundCases);

            await _context.SaveChangesAsync();
        }
    }
}

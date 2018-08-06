using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.RefundCase;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Core.Services
{
    public class RefundCaseService : IRefundCaseService
    {
        private readonly IUtilityService _utilityService;
        private readonly UserManager<RefundeoUser> _userManager;
        private readonly IBlobStorageService _blobStorageService;
        private readonly RefundeoDbContext _context;

        public RefundCaseService(
            IUtilityService utilityService,
            UserManager<RefundeoUser> userManager,
            IBlobStorageService blobStorageService,
            RefundeoDbContext context)
        {
            _utilityService = utilityService;
            _userManager = userManager;
            _blobStorageService = blobStorageService;
            _context = context;
        }

        public async Task<ObjectResult> GenerateRefundCaseDtoResponseAsync(IEnumerable<RefundCase> refundCases,
            RefundeoUser callingUser)
        {
            if (await _userManager.IsInRoleAsync(callingUser, RefundeoConstants.RoleAdmin))
            {
                var dtos = new List<RefundCaseAdminDto>();
                foreach (var refundCase in refundCases)
                {
                    dtos.Add(await ConvertRefundCaseToAdminDtoAsync(refundCase));
                }

                return new ObjectResult(dtos);
            }
            else
            {
                var dtos = new List<RefundCaseDto>();
                foreach (var refundCase in refundCases)
                {
                    dtos.Add(await ConvertRefundCaseToDtoAsync(refundCase));
                }

                return new ObjectResult(dtos);
            }
        }

        public async Task<ObjectResult> GenerateRefundCaseDtoResponseAsync(RefundCase refundCase,
            RefundeoUser callingUser)
        {
            if (await _userManager.IsInRoleAsync(callingUser, RefundeoConstants.RoleAdmin))
            {
                var dto = await ConvertRefundCaseToAdminDtoAsync(refundCase);
                return new ObjectResult(dto);
            }
            else
            {
                var dto = await ConvertRefundCaseToDtoAsync(refundCase);
                return new ObjectResult(dto);
            }
        }

        public RefundCaseSimpleDto ConvertRefundCaseToDtoSimple(RefundCase refundCase)
        {
            return new RefundCaseSimpleDto
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
                DateCreated = refundCase.DateCreated,
                DateRequested = refundCase.DateRequested,
                ReceiptNumber = refundCase.ReceiptNumber,
                Customer = _utilityService.ConvertCustomerInformationToSimpleDto(refundCase.CustomerInformation)
            };
        }

        public async Task<RefundCaseAdminDto> ConvertRefundCaseToAdminDtoAsync(RefundCase refundCase)
        {
            return new RefundCaseAdminDto
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
                VatFormImage = await _utilityService.ConvertBlobPathToBase64Async(refundCase.VATFormImage),
                ReceiptImage = await _utilityService.ConvertBlobPathToBase64Async(refundCase.ReceiptImage),
                DateCreated = refundCase.DateCreated,
                DateRequested = refundCase.DateRequested,
                ReceiptNumber = refundCase.ReceiptNumber,
                Customer = await _utilityService.ConvertCustomerInformationToDtoAsync(refundCase.CustomerInformation),
                Merchant = await _utilityService.ConvertMerchantInformationToDtoAsync(refundCase.MerchantInformation),
            };
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
                Merchant = _utilityService.ConvertMerchantInformationToSimpleDto(refundCase.MerchantInformation),
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

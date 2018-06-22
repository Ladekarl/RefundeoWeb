using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.QRCode;
using Refundeo.Core.Models.RefundCase;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Controllers.Merchant
{
    [Authorize(Roles = RefundeoConstants.RoleMerchant)]
    [Route("/api/merchant/refundcase")]
    public class MerchantRefundCaseController : Controller
    {
        private readonly RefundeoDbContext _context;
        private readonly IRefundCaseService _refundCaseService;
        private readonly IUtilityService _utilityService;
        private readonly IPaginationService<RefundCase> _paginationService;
        private readonly IEmailService _emailService;
        private readonly IOptions<StorageAccountOptions> _optionsAccessor;
        private readonly IBlobStorageService _blobStorageService;

        public MerchantRefundCaseController(RefundeoDbContext context, IRefundCaseService refundCaseService,
            IUtilityService utilityService, IPaginationService<RefundCase> paginationService,
            IEmailService emailService, IOptions<StorageAccountOptions> optionsAccessor,
            IBlobStorageService blobStorageService)
        {
            _context = context;
            _refundCaseService = refundCaseService;
            _utilityService = utilityService;
            _paginationService = paginationService;
            _emailService = emailService;
            _optionsAccessor = optionsAccessor;
            _blobStorageService = blobStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMerchantRefundCases()
        {
            var user = await _utilityService.GetCallingUserAsync(Request);

            if (user == null)
            {
                return Forbid();
            }

            var refundCases = await _context.RefundCases
                .Include(r => r.MerchantInformation.Merchant)
                .Include(r => r.CustomerInformation.Customer)
                .Where(r => r.MerchantInformation.Merchant.Id == user.Id)
                .ToListAsync();

            if (refundCases == null)
            {
                return NotFound();
            }

            return await _refundCaseService.GenerateRefundCaseDtoResponseAsync(refundCases);
        }

        [HttpGet("{first}/{amount}/{sortBy}/{dir}/{filterBy}")]
        public async Task<IActionResult> GetPaginatedMerchantRefundCases(int first, int amount, string sortBy,
            string dir, string filterBy)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);

            if (user == null)
            {
                return Forbid();
            }

            var query = _context.RefundCases
                .Where(r => r.MerchantInformation.Merchant.Id == user.Id);

            query = _paginationService.SortAndFilter(query, sortBy, dir, filterBy);

            var totalRecords = await query
                .AsNoTracking()
                .CountAsync();

            query = query
                .Include(r => r.MerchantInformation.Merchant)
                .Include(r => r.CustomerInformation.Customer);

            var refundCases = await _paginationService
                .Paginate(query, first, amount)
                .AsNoTracking()
                .ToListAsync();

            if (refundCases == null)
            {
                return NotFound();
            }

            var dtos = new List<RefundCaseDto>();
            foreach (var refundCase in refundCases)
            {
                dtos.Add(await _refundCaseService.ConvertRefundCaseToDtoAsync(refundCase));
            }

            return new ObjectResult(new
            {
                totalRecords,
                refundCases = dtos
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMerchantRefundCaseById(long id)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Forbid();
            }

            var refundCase = await _context.RefundCases
                .Include(r => r.CustomerInformation.Customer)
                .Include(r => r.MerchantInformation.Merchant)
                .Where(r => r.Id == id && r.MerchantInformation.Merchant.Id == user.Id)
                .FirstOrDefaultAsync();

            if (refundCase == null)
            {
                return NotFound();
            }

            return await _refundCaseService.GenerateRefundCaseDtoResponseAsync(refundCase);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMerchantRefundCase([FromBody] CreateRefundCaseDto model)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var merchantInformation = await _context.MerchantInformations
                .Include(m => m.Address)
                .Include(m => m.Merchant)
                .Where(c => c.Merchant.Id == user.Id)
                .FirstOrDefaultAsync();

            if (merchantInformation == null)
            {
                return NotFound("Merchant not found");
            }

            var customerInformation =
                await _context.CustomerInformations
                    .Include(c => c.Customer)
                    .Where(c => c.Customer.Id == model.CustomerId)
                    .FirstOrDefaultAsync();

            if (customerInformation == null)
            {
                return BadRequest("Customer not found");
            }

            var refundAmount = 0.20 * model.Amount * ((95 - merchantInformation.RefundPercentage) / 100.0);

            var refundCase = new RefundCase
            {
                Amount = model.Amount,
                RefundAmount = refundAmount,
                DateCreated = DateTime.UtcNow,
                MerchantInformation = merchantInformation,
                CustomerInformation = customerInformation,
                ReceiptNumber = model.ReceiptNumber
            };

            var refundCaseResult = await _context.RefundCases.AddAsync(refundCase);

            await _context.SaveChangesAsync();

            var qrCode = _utilityService.GenerateQrCode(model.QrCodeHeight, model.QrCodeWidth, model.QrCodeMargin,
                new QRCodeRefundCaseDto
                {
                    RefundCaseId = refundCase.Id
                });

            var logoContainerName = _optionsAccessor.Value.QrCodesContainerNameOption;
            refundCase.QRCode = await _blobStorageService.UploadAsync(logoContainerName,
                $"{refundCase.Id}-{refundCase.ReceiptNumber}", _utilityService.ConvertByteArrayToBase64(qrCode),
                "image/png");

            var signaturesContainerName = _optionsAccessor.Value.SignaturesContainerNameOption;
            refundCase.MerchantSignature = await _blobStorageService.UploadAsync(signaturesContainerName,
                $"{refundCase.Id}-merchantsignature", model.MerchantSignature,
                "image/png");
            refundCase.CustomerSignature = await _blobStorageService.UploadAsync(signaturesContainerName,
                $"{refundCase.Id}-customersignature", model.CustomerSignature,
                "image/png");


            _context.RefundCases.Update(refundCase);

            await _context.SaveChangesAsync();

            await _emailService.SendVATMailAsync(refundCase, customerInformation.Email);

            return await _refundCaseService.GenerateRefundCaseDtoResponseAsync(refundCaseResult.Entity);
        }

        // Do Merchants ever need to put refund case? This would make it unsafe since clients could alter this.
        // We already have accept endpoint for accepting refund cases

        // [HttpPut("{id}")]
        // public async Task<IActionResult> UpdateMerchantRefundCase(long id, [FromBody] RefundCaseDto model)
        // {
        //     var user = await _utilityService.GetCallingUserAsync(Request);
        //     if (user == null)
        //     {
        //         return Forbid();
        //     }
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest();
        //     }

        //     var refundCaseToUpdate = await context.RefundCases
        //     .Include(r => r.Documentation)
        //     .Include(r => r.QRCode)
        //     .Include(r => r.MerchantInformation)
        //     .ThenInclude(i => i.Merchant)
        //     .FirstOrDefaultAsync(r => r.Id == id && r.MerchantInformation.Merchant.Id == user.Id);

        //     if (refundCaseToUpdate == null)
        //     {
        //         return NotFound();
        //     }

        //     refundCaseToUpdate.Amount = model.Amount;
        //     refundCaseToUpdate.Documentation.Image = ConvertBase64ToByteArray(model.Documentation);
        //     refundCaseToUpdate.QRCode.Image = ConvertBase64ToByteArray(model.QRCode);
        //     refundCaseToUpdate.RefundAmount = model.RefundAmount;
        //     refundCaseToUpdate.IsAccepted = model.IsAccepted;
        //     refundCaseToUpdate.IsRejected = model.IsRejected
        //     refundCaseToUpdate.IsRequested = model.IsRequested;

        //     context.RefundCases.Update(refundCaseToUpdate);
        //     await context.SaveChangesAsync();
        //     return NoContent();
        // }

        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptRefund(long id, [FromBody] AcceptRefundCaseDto model)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var refundCaseToUpdate = await _context.RefundCases
                .Include(r => r.MerchantInformation.Merchant)
                .FirstOrDefaultAsync(r => r.Id == id && r.MerchantInformation.Merchant.Id == user.Id);

            if (refundCaseToUpdate == null)
            {
                return NotFound();
            }

            refundCaseToUpdate.IsAccepted = model.IsAccepted;
            refundCaseToUpdate.IsRejected = !model.IsAccepted;

            _context.RefundCases.Update(refundCaseToUpdate);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // TODO: Shouldn't this only be an option if the refund case has been processed?

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMerchantRefundCase(long id)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Forbid();
            }

            var refundCase = await _context.RefundCases
                .Include(r => r.MerchantInformation.Merchant)
                .FirstOrDefaultAsync(r => r.Id == id && r.MerchantInformation.Merchant.Id == user.Id);
            if (refundCase == null)
            {
                return NotFound();
            }

            _context.RefundCases.Remove(refundCase);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

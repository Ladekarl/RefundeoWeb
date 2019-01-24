using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.QRCode;
using Refundeo.Core.Models.RefundCase;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Controllers.Admin
{
    [Authorize(Roles = RefundeoConstants.RoleAdmin)]
    [Route("/api/admin/refundcase")]
    public class AdminRefundCaseController : Controller
    {
        private readonly RefundeoDbContext _context;
        private readonly IRefundCaseService _refundCaseService;
        private readonly IUtilityService _utilityService;
        private readonly IOptions<StorageAccountOptions> _optionsAccessor;
        private readonly IBlobStorageService _blobStorageService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AdminRefundCaseController> _logger;

        public AdminRefundCaseController(RefundeoDbContext context, IRefundCaseService refundCaseService,
            IUtilityService utilityService, IOptions<StorageAccountOptions> optionsAccessor,
            IBlobStorageService blobStorageService, INotificationService notificationService,
            ILogger<AdminRefundCaseController> logger)
        {
            _context = context;
            _refundCaseService = refundCaseService;
            _utilityService = utilityService;
            _optionsAccessor = optionsAccessor;
            _blobStorageService = blobStorageService;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRefundCases()
        {
            var refundCases = await _context.RefundCases
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Merchants)
                .ToListAsync();

            if (refundCases == null)
            {
                return NotFound();
            }

            var dtos = new List<RefundCaseAdminDto>();
            foreach (var refundCase in refundCases)
            {
                dtos.Add(await _refundCaseService.ConvertRefundCaseToAdminDtoAsync(refundCase));
            }

            return new ObjectResult(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRefundCaseById(long id)
        {
            var refundCase = await _context.RefundCases
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Address)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Merchants)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Address)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Location)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (refundCase == null)
            {
                return NotFound();
            }

            return await _refundCaseService.GenerateRefundCaseDtoResponseAsync(refundCase,
                await _utilityService.GetCallingUserAsync(Request));
        }

        [HttpPost]
        public async Task<IActionResult> CreateRefundCase([FromBody] AdminCreateRefundCaseDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var merchantInformation = await _context.MerchantInformations
                .Include(i => i.Merchants)
                .Include(i => i.FeePoints)
                .Where(i => i.Merchants.Any(m => m.Id == model.MerchantId))
                .FirstOrDefaultAsync();

            if (merchantInformation == null)
            {
                return BadRequest("Merchant not found");
            }

            CustomerInformation customerInformation = null;
            if (model.CustomerId != null)
            {
                customerInformation = await _context.CustomerInformations
                    .Where(i => i.Customer.Id == model.CustomerId)
                    .FirstOrDefaultAsync();

                if (customerInformation == null)
                {
                    return BadRequest("Customer not found");
                }
            }

            var feePoint = merchantInformation.FeePoints.FirstOrDefault(f =>
                               f.Start <= model.Amount && f.End.HasValue && f.End.Value > model.Amount) ??
                           merchantInformation.FeePoints.FirstOrDefault(f =>
                               f.Start <= model.Amount && !f.End.HasValue);

            if (feePoint == null)
            {
                return BadRequest("No feepoint found");
            }

            var refundAmount = model.Amount * (feePoint.RefundPercentage / 100);
            var vatAmount = model.Amount - model.Amount / (1 + merchantInformation.VATRate / 100);
            var adminAmount = vatAmount * (feePoint.AdminFee / 100);
            var merchantAmount = vatAmount * (feePoint.MerchantFee / 100);

            var refundCase = new RefundCase
            {
                Amount = model.Amount,
                RefundAmount = refundAmount,
                MerchantInformation = merchantInformation,
                CustomerInformation = customerInformation,
                VATAmount = vatAmount,
                MerchantAmount = merchantAmount,
                AdminAmount = adminAmount,
                DateCreated = DateTime.UtcNow,
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
                $"{refundCase.Id}", _utilityService.ConvertByteArrayToBase64(qrCode),
                "image/png");

            _context.RefundCases.Update(refundCase);
            await _context.SaveChangesAsync();

            var text =
                await _context.Languages.Where(t => t.Key == customerInformation.Language).FirstOrDefaultAsync() ??
                await _context.Languages.Where(t => t.Key == "en").FirstOrDefaultAsync();

            _notificationService.SendNotificationAsync(model.CustomerId, merchantInformation.CompanyName,
                text.RefundCreatedText);

            return await _refundCaseService.GenerateRefundCaseDtoResponseAsync(refundCaseResult.Entity,
                await _utilityService.GetCallingUserAsync(Request));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRefundCase(long id, [FromBody] AdminUpdateRefundCaseDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var refundCaseToUpdate = await _context.RefundCases
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Address)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Merchants)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Address)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Location)
                .Include(r => r.MerchantInformation)
                .ThenInclude(m => m.MerchantInformationTags)
                .ThenInclude(m => m.Tag)
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();

            if (refundCaseToUpdate == null)
            {
                return NotFound();
            }

            var merchantInformation =
                await _context.MerchantInformations.FirstOrDefaultAsync(i =>
                    i.Merchants.Any(m => m.Id == model.MerchantId));
            var customerInformation =
                await _context.CustomerInformations.FirstOrDefaultAsync(i => i.Customer.Id == model.CustomerId);

            if (merchantInformation == null || customerInformation == null)
            {
                return BadRequest();
            }

            refundCaseToUpdate.Amount = model.Amount;
            refundCaseToUpdate.CustomerInformation = customerInformation;
            refundCaseToUpdate.MerchantInformation = merchantInformation;
            refundCaseToUpdate.IsRequested = model.IsRequested;
            refundCaseToUpdate.IsAccepted = model.IsAccepted;
            refundCaseToUpdate.IsRejected = model.IsRejected;
            refundCaseToUpdate.RefundAmount = model.RefundAmount;
            refundCaseToUpdate.AdminAmount = model.AdminAmount;
            refundCaseToUpdate.MerchantAmount = model.MerchantAmount;
            refundCaseToUpdate.VATAmount = model.VatAmount;
            refundCaseToUpdate.ReceiptNumber = model.ReceiptNumber;

            _context.RefundCases.Update(refundCaseToUpdate);
            await _context.SaveChangesAsync();

            var text =
                await _context.Languages.Where(t => t.Key == customerInformation.Language).FirstOrDefaultAsync() ??
                await _context.Languages.Where(t => t.Key == "en").FirstOrDefaultAsync();

            _notificationService.SendNotificationAsync(refundCaseToUpdate.CustomerInformation.Customer.Id,
                refundCaseToUpdate.MerchantInformation.CompanyName,
                text.RefundUpdateText);

            return new NoContentResult();
        }

        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptRefund(long id, [FromBody] AcceptRefundCaseDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var refundCaseToUpdate = await _context.RefundCases
                .Include(r => r.MerchantInformation)
                .Include(r => r.CustomerInformation)
                .ThenInclude(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (refundCaseToUpdate == null)
            {
                return NotFound();
            }

            refundCaseToUpdate.IsAccepted = model.IsAccepted;
            refundCaseToUpdate.IsRejected = !model.IsAccepted;

            _context.RefundCases.Update(refundCaseToUpdate);
            await _context.SaveChangesAsync();

            var text = await _context.Languages.Where(t => t.Key == refundCaseToUpdate.CustomerInformation.Language)
                           .FirstOrDefaultAsync() ??
                       await _context.Languages.Where(t => t.Key == "en").FirstOrDefaultAsync();

            if (refundCaseToUpdate.CustomerInformation?.Customer?.Id != null &&
                refundCaseToUpdate.MerchantInformation?.CompanyName != null)
            {
                _logger.LogInformation(LoggingEvents.SendItem, "Sending notification for {ID}",
                    refundCaseToUpdate.CustomerInformation.Customer.Id,
                    refundCaseToUpdate.MerchantInformation.CompanyName, text.RefundUpdateText);
                await _notificationService.SendNotificationAsync(refundCaseToUpdate.CustomerInformation.Customer.Id,
                    refundCaseToUpdate.MerchantInformation.CompanyName,
                    text.RefundUpdateText);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRefundCase(long id)
        {
            var refundCase = await _context.RefundCases.FirstOrDefaultAsync(r => r.Id == id);
            if (refundCase == null)
            {
                return NotFound();
            }

            _context.RefundCases.Remove(refundCase);
            await _context.SaveChangesAsync();
            return new NoContentResult();
        }
    }
}

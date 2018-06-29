using System;
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

        public AdminRefundCaseController(RefundeoDbContext context, IRefundCaseService refundCaseService,
            IUtilityService utilityService, IOptions<StorageAccountOptions> optionsAccessor,
            IBlobStorageService blobStorageService, INotificationService notificationService)
        {
            _context = context;
            _refundCaseService = refundCaseService;
            _utilityService = utilityService;
            _optionsAccessor = optionsAccessor;
            _blobStorageService = blobStorageService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRefundCases()
        {
            var refundCases = await _context.RefundCases
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Address)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Merchant)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Address)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Location)
                .ToListAsync();

            if (refundCases == null)
            {
                return NotFound();
            }

            return await _refundCaseService.GenerateRefundCaseDtoResponseAsync(refundCases);
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
                .ThenInclude(i => i.Merchant)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Address)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Location)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (refundCase == null)
            {
                return NotFound();
            }

            return await _refundCaseService.GenerateRefundCaseDtoResponseAsync(refundCase);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRefundCase([FromBody] AdminCreateRefundCaseDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var merchantInformation = await _context.MerchantInformations
                .Include(i => i.Merchant)
                .Where(i => i.Merchant.Id == model.MerchantId)
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

            var factor = merchantInformation.RefundPercentage / 100.0;
            var refundAmount = factor * model.Amount;

            var refundCase = new RefundCase
            {
                Amount = model.Amount,
                RefundAmount = refundAmount,
                MerchantInformation = merchantInformation,
                CustomerInformation = customerInformation,
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

            var text = await _context.Languages.Where(t => t.Key == customerInformation.Language).FirstOrDefaultAsync() ??
                       await _context.Languages.Where(t => t.Key == "en").FirstOrDefaultAsync();

            _notificationService.SendNotificationAsync(model.CustomerId, merchantInformation.CompanyName, text.RefundCreatedText);

            return await _refundCaseService.GenerateRefundCaseDtoResponseAsync(refundCaseResult.Entity);
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
                .ThenInclude(i => i.Merchant)
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
                await _context.MerchantInformations.FirstOrDefaultAsync(i => i.Merchant.Id == model.MerchantId);
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
            refundCaseToUpdate.ReceiptNumber = model.ReceiptNumber;

            _context.RefundCases.Update(refundCaseToUpdate);
            await _context.SaveChangesAsync();

            var text = await _context.Languages.Where(t => t.Key == customerInformation.Language).FirstOrDefaultAsync() ??
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

            var text = await _context.Languages.Where(t => t.Key == refundCaseToUpdate.CustomerInformation.Language).FirstOrDefaultAsync() ??
                       await _context.Languages.Where(t => t.Key == "en").FirstOrDefaultAsync();

            _notificationService.SendNotificationAsync(refundCaseToUpdate.CustomerInformation.Customer.Id,
                refundCaseToUpdate.MerchantInformation.CompanyName,
                text.RefundUpdateText);

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

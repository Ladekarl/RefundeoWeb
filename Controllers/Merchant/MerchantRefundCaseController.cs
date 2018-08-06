using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly INotificationService _notificationService;

        public MerchantRefundCaseController(RefundeoDbContext context, IRefundCaseService refundCaseService,
            IUtilityService utilityService, IPaginationService<RefundCase> paginationService,
            IEmailService emailService, IOptions<StorageAccountOptions> optionsAccessor,
            IBlobStorageService blobStorageService, INotificationService notificationService)
        {
            _context = context;
            _refundCaseService = refundCaseService;
            _utilityService = utilityService;
            _paginationService = paginationService;
            _emailService = emailService;
            _optionsAccessor = optionsAccessor;
            _blobStorageService = blobStorageService;
            _notificationService = notificationService;
        }

        [Authorize(Roles = RefundeoConstants.RoleMerchant + "," + RefundeoConstants.RoleAdmin)]
        [HttpGet]
        public async Task<IActionResult> GetAllMerchantRefundCases()
        {
            var user = await _utilityService.GetCallingUserAsync(Request);

            if (user == null)
            {
                return Forbid();
            }

            var refundCases = await _context.RefundCases
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Merchants)
                .Include(r => r.CustomerInformation)
                .Where(r => r.MerchantInformation.Merchants.Any(m => m.Id == user.Id))
                .AsNoTracking()
                .ToListAsync();

            if (refundCases == null)
            {
                return NotFound();
            }

            var dtos = new List<RefundCaseSimpleDto>();
            foreach (var refundCase in refundCases)
            {
                dtos.Add(_refundCaseService.ConvertRefundCaseToDtoSimple(refundCase));
            }

            return new ObjectResult(dtos);
        }

        [Authorize(Roles = RefundeoConstants.RoleMerchant + "," + RefundeoConstants.RoleAdmin)]
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
                .Include(r => r.MerchantInformation)
                .ThenInclude(m => m.Merchants)
                .Where(r => r.MerchantInformation.Merchants.Any(m => m.Id == user.Id));

            query = _paginationService.SortAndFilter(query, sortBy, dir, filterBy);

            var totalRecords = await query
                .AsNoTracking()
                .CountAsync();

            query = query
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Merchants)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Address)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Location)
                .Include(r => r.MerchantInformation)
                .ThenInclude(m => m.MerchantInformationTags)
                .ThenInclude(m => m.Tag);

            var refundCases = await _paginationService
                .Paginate(query, first, amount)
                .AsNoTracking()
                .ToListAsync();

            if (refundCases == null)
            {
                return NotFound();
            }

            var dtos = new List<RefundCaseSimpleDto>();
            foreach (var refundCase in refundCases)
            {
                refundCase.CustomerSignature = null;
                refundCase.MerchantSignature = null;
                refundCase.QRCode = null;
                refundCase.CustomerInformation = null;
                dtos.Add(_refundCaseService.ConvertRefundCaseToDtoSimple(refundCase));
            }

            return new ObjectResult(new
            {
                totalRecords,
                refundCases = dtos
            });
        }

        [Authorize(Roles = RefundeoConstants.RoleMerchant + "," + RefundeoConstants.RoleAdmin)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMerchantRefundCaseById(long id)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Forbid();
            }

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
                .Where(r => r.Id == id && r.MerchantInformation.Merchants.Any(m => m.Id == user.Id))
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (refundCase == null)
            {
                return NotFound();
            }

            return await _refundCaseService.GenerateRefundCaseDtoResponseAsync(refundCase, user);
        }

        [Authorize(Roles = RefundeoConstants.RoleMerchant + "," +
                           RefundeoConstants.RoleAdmin + "," +
                           RefundeoConstants.RoleAttachedMerchant)]
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
                .Include(m => m.Merchants)
                .Where(c => c.Merchants.Any(m => m.Id == user.Id))
                .FirstOrDefaultAsync();

            if (merchantInformation == null)
            {
                return NotFound("Merchant not found");
            }

            var customerInformation =
                await _context.CustomerInformations
                    .Include(c => c.Customer)
                    .Include(c => c.Address)
                    .Where(c => c.Customer.Id == model.CustomerId)
                    .FirstOrDefaultAsync();

            if (customerInformation == null)
            {
                return BadRequest("Customer not found");
            }

            var refundAmount = model.Amount * (merchantInformation.RefundPercentage / 100);
            var vatAmount = model.Amount - model.Amount / (1 + merchantInformation.VATRate / 100);
            var adminAmount = vatAmount * (merchantInformation.AdminFee / 100);
            var merchantAmount = vatAmount * (merchantInformation.MerchantFee / 100);

            var refundCase = new RefundCase
            {
                Amount = model.Amount,
                RefundAmount = refundAmount,
                AdminAmount = adminAmount,
                MerchantAmount = merchantAmount,
                VATAmount = vatAmount,
                DateCreated = DateTime.UtcNow,
                MerchantInformation = merchantInformation,
                CustomerInformation = customerInformation,
                ReceiptNumber = model.ReceiptNumber
            };

            await _context.RefundCases.AddAsync(refundCase);

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

            var text =
                await _context.Languages.Where(t => t.Key == customerInformation.Language).FirstOrDefaultAsync() ??
                await _context.Languages.Where(t => t.Key == "en").FirstOrDefaultAsync();

            _emailService.SendVATMail(ControllerContext, refundCase, customerInformation.Email);

            _notificationService.SendNotificationAsync(customerInformation.Customer.Id,
                refundCase.MerchantInformation.CompanyName, text.RefundCreatedText);

            return NoContent();
        }
    }
}

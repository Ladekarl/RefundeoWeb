using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Refundeo.Core.Data;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.RefundCase;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Controllers.User
{
    [Authorize(Roles = RefundeoConstants.RoleUser + "," + RefundeoConstants.RoleAdmin)]
    [Route("/api/user/refundcase")]
    public class UserRefundCaseController : Controller
    {
        private readonly RefundeoDbContext _context;
        private readonly IRefundCaseService _refundCaseService;
        private readonly IUtilityService _utilityService;
        private readonly IOptions<StorageAccountOptions> _optionsAccessor;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IEmailService _emailService;

        public UserRefundCaseController(RefundeoDbContext context, IRefundCaseService refundCaseService,
            IUtilityService utilityService, IOptions<StorageAccountOptions> optionsAccessor,
            IBlobStorageService blobStorageService, IEmailService emailService,
            INotificationService notificationService)
        {
            _context = context;
            _refundCaseService = refundCaseService;
            _utilityService = utilityService;
            _optionsAccessor = optionsAccessor;
            _blobStorageService = blobStorageService;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUserRefundCases()
        {
            var user = await _utilityService.GetCallingUserAsync(Request);

            if (user == null)
            {
                return Forbid();
            }

            var refundCases = await _context.RefundCases
                .Where(r => r.CustomerInformation.Customer == user)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Merchants)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Address)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Location)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .AsNoTracking()
                .ToListAsync();

            if (refundCases == null)
            {
                return NotFound();
            }

            var dtos = new List<RefundCaseDto>();
            foreach (var refundCase in refundCases)
            {
                refundCase.CustomerSignature = null;
                refundCase.MerchantSignature = null;
                refundCase.QRCode = null;
                if (refundCase.IsRequested)
                {
                    refundCase.ReceiptImage = null;
                    refundCase.VATFormImage = null;
                }
                dtos.Add(await _refundCaseService.ConvertRefundCaseToDtoAsync(refundCase));
            }

            return new ObjectResult(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserRefundCaseById(long id)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Forbid();
            }

            var refundCase = await _context.RefundCases
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Merchants)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Address)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Location)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Address)
                .Where(r => r.Id == id && r.CustomerInformation.Customer == user)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (refundCase == null)
            {
                return NotFound();
            }

            return await _refundCaseService.GenerateRefundCaseDtoResponseAsync(refundCase, user);
        }

        [HttpPost("{id}/doc")]
        public async Task<IActionResult> UploadDocumentation(long id, [FromBody] DocumentationDto model)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Forbid();
            }

            if (!ModelState.IsValid || string.IsNullOrEmpty(model.ReceiptImage) ||
                string.IsNullOrEmpty(model.ReceiptImageType) || string.IsNullOrEmpty(model.VatFormImageType) ||
                string.IsNullOrEmpty(model.VatFormImage) ||
                !RefundeoConstants.ValidImageTypes.Contains(model.VatFormImageType) ||
                !RefundeoConstants.ValidImageTypes.Contains(model.ReceiptImageType))
            {
                return BadRequest();
            }

            var refundCaseToUpdate = await _context.RefundCases
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Address)
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerInformation.Customer == user);

            if (refundCaseToUpdate == null)
            {
                return NotFound();
            }

            var containerName = _optionsAccessor.Value.DocumentationContainerNameOption;

            refundCaseToUpdate.ReceiptImage = await _blobStorageService.UploadAsync(containerName,
                $"{refundCaseToUpdate.Id}_{refundCaseToUpdate.ReceiptNumber}_receipt", model.ReceiptImage,
                model.ReceiptImageType);

            refundCaseToUpdate.VATFormImage = await _blobStorageService.UploadAsync(containerName,
                $"{refundCaseToUpdate.Id}_{refundCaseToUpdate.ReceiptNumber}_vatform", model.VatFormImage,
                model.VatFormImageType);

            await _context.SaveChangesAsync();
            _context.RefundCases.Update(refundCaseToUpdate);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/request")]
        public async Task<IActionResult> RequestRefund(long id, [FromBody] RequestRefundDto model)
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
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .Where(r => r.Id == id && r.CustomerInformation.Customer == user)
                .FirstOrDefaultAsync();

            if (refundCaseToUpdate == null)
            {
                return NotFound();
            }

            if (refundCaseToUpdate.ReceiptImage == null || refundCaseToUpdate.VATFormImage == null)
            {
                return BadRequest("No documentation found");
            }

            refundCaseToUpdate.IsRequested = model.IsRequested;
            refundCaseToUpdate.DateRequested = DateTime.UtcNow;
            _context.RefundCases.Update(refundCaseToUpdate);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/email")]
        public async Task<IActionResult> SendRefundCaseEmail(long id, [FromBody] EmailDto model)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Forbid();
            }

            if (model.Email == null)
            {
                return BadRequest();
            }

            var refundCase = await _context.RefundCases
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Address)
                .Include(i => i.MerchantInformation)
                .ThenInclude(i => i.Address)
                .Where(r => r.Id == id && r.CustomerInformation.Customer == user)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (refundCase == null)
            {
                return NotFound();
            }

            await _emailService.SendVATMailAsync(refundCase, model.Email);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMerchantRefundCase(long id)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Forbid();
            }

            var refundCase = await _context.RefundCases
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerInformation.Customer == user);

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

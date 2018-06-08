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

        public UserRefundCaseController(RefundeoDbContext context, IRefundCaseService refundCaseService,
            IUtilityService utilityService, IOptions<StorageAccountOptions> optionsAccessor,
            IBlobStorageService blobStorageService)
        {
            _context = context;
            _refundCaseService = refundCaseService;
            _utilityService = utilityService;
            _optionsAccessor = optionsAccessor;
            _blobStorageService = blobStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUserRefundCases()
        {
            var user = await _utilityService.GetCallingUserAsync(Request);

            if (user == null)
            {
                return Forbid();
            }

            var refundCases = _context.RefundCases
                .Where(r => r.CustomerInformation.Customer == user)
                .Include(r => r.Documentation)
                .Include(r => r.QRCode)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Merchant)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Address)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Location)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .AsNoTracking();

            if (refundCases == null)
            {
                return NotFound();
            }

            return await _refundCaseService.GenerateRefundCaseDtoResponseAsync(refundCases);
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
                .Include(r => r.QRCode)
                .Include(r => r.Documentation)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Merchant)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Address)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Location)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Address)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerInformation.Customer == user);

            if (refundCase == null)
            {
                return NotFound();
            }

            return await _refundCaseService.GenerateRefundCaseDtoResponseAsync(refundCase);
        }

        [HttpPost("{id}/doc")]
        public async Task<IActionResult> UploadDocumentation(long id, [FromBody] DocumentationDto model)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Forbid();
            }

            if (!ModelState.IsValid || string.IsNullOrEmpty(model.Image) || string.IsNullOrEmpty(model.ImageName) ||
                string.IsNullOrEmpty(model.ImageType) || !RefundeoConstants.ValidImageTypes.Contains(model.ImageType))
            {
                return BadRequest();
            }

            var refundCaseToUpdate = await _context.RefundCases
                .Include(r => r.Documentation)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Address)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerInformation.Customer == user);

            if (refundCaseToUpdate == null)
            {
                return NotFound();
            }

            var containerName = _optionsAccessor.Value.DocumentationContainerNameOption;

            var path = await _blobStorageService.UploadAsync(containerName, model.ImageName, model.Image,
                model.ImageType);

            var documentation = new Documentation
            {
                Image = path
            };

            await _context.Documentations.AddAsync(documentation);
            await _context.SaveChangesAsync();
            refundCaseToUpdate.Documentation = documentation;
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
                .Include(r => r.Documentation)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Address)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerInformation.Customer == user);

            if (refundCaseToUpdate == null)
            {
                return NotFound();
            }

            if (refundCaseToUpdate.Documentation == null)
            {
                return BadRequest("No documentation found");
            }

            refundCaseToUpdate.IsRequested = model.IsRequested;
            refundCaseToUpdate.DateRequested = DateTime.UtcNow;
            _context.RefundCases.Update(refundCaseToUpdate);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}/claim")]
        public async Task<IActionResult> ClaimRefundCase(long id)
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
                .ThenInclude(c => c.Address)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (refundCaseToUpdate == null)
            {
                return NotFound("Refund case not found");
            }

            if (refundCaseToUpdate.CustomerInformation != null)
            {
                return BadRequest("Refund case already claimed");
            }

            var customerInformation =
                await _context.CustomerInformations.FirstOrDefaultAsync(i => i.Customer.Id == user.Id);

            if (customerInformation == null)
            {
                return NotFound("Customer not found");
            }

            refundCaseToUpdate.CustomerInformation = customerInformation;
            _context.RefundCases.Update(refundCaseToUpdate);
            await _context.SaveChangesAsync();
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

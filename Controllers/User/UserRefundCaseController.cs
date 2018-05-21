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
    [Authorize(Roles = RefundeoConstants.RoleUser)]
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
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer);

            if (refundCases == null)
            {
                return NotFound();
            }

            return _refundCaseService.GenerateRefundCaseDtoResponse(refundCases);
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
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerInformation.Customer == user);

            if (refundCase == null)
            {
                return NotFound();
            }

            return _refundCaseService.GenerateRefundCaseDtoResponse(refundCase);
        }

        [HttpPost("doc")]
        public async Task<IActionResult> UploadDocumentation(DocementationDto model)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Forbid();
            }

            if (!ModelState.IsValid || model.File == null || model.File.Length == 0)
            {
                return BadRequest();
            }

            var refundCaseToUpdate = await _context.RefundCases
                .Include(r => r.Documentation)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .FirstOrDefaultAsync(r => r.Id == model.RefundCaseId && r.CustomerInformation.Customer == user);

            if (refundCaseToUpdate == null)
            {
                return NotFound();
            }

            var blobName = model.File.FileName;
            var fileStream = model.File.OpenReadStream();

            var containerName = _optionsAccessor.Value.DocumentationContainerNameOption;

            await _blobStorageService.UploadAsync(containerName, blobName, fileStream);

            var blob = await _blobStorageService.GetBlockBlobAsync(containerName, blobName);

            var documentation = new Documentation
            {
                Image = blob.Uri.AbsoluteUri
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
            return new NoContentResult();
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

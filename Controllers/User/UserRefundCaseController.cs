using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.QRCode;
using Refundeo.Core.Models.RefundCase;
using Refundeo.Core.Services.Interfaces;
using ZXing;
using ZXing.QrCode;

namespace Refundeo.Controllers.User
{
    [Authorize(Roles = RefundeoConstants.ROLE_USER)]
    [Route("/api/user/refundcase")]
    public class UserRefundCaseController : Controller
    {
        private RefundeoDbContext _context;
        private IRefundCaseService _refundCaseService;
        private IUtilityService _utilityService;
        public UserRefundCaseController(RefundeoDbContext context, IRefundCaseService refundCaseService, IUtilityService utilityService)
        {
            _context = context;
            _refundCaseService = refundCaseService;
            _utilityService = utilityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUserRefundCases()
        {
            var user = await _utilityService.GetCallingUserAsync(Request);

            if (user == null)
            {
                return Unauthorized();
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

            return _refundCaseService.GenerateRefundCaseDTOResponse(refundCases);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserRefundCaseById(long id)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Unauthorized();
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

            return _refundCaseService.GenerateRefundCaseDTOResponse(refundCase);
        }

        [HttpPost("{id}/doc")]
        public async Task<IActionResult> UploadDocumentation(long id, [FromBody] DocementationDTO model)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Unauthorized();
            }
            if (!ModelState.IsValid || string.IsNullOrEmpty(model.Image))
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

            var documentation = new Documentation();

            try
            {
                documentation.Image = _refundCaseService.ConvertBase64ToByteArray(model.Image);
            }
            catch (System.FormatException)
            {
                return BadRequest("Image should be base64 encoded");
            }

            await _context.Documentations.AddAsync(documentation);
            await _context.SaveChangesAsync();
            refundCaseToUpdate.Documentation = documentation;
            _context.RefundCases.Update(refundCaseToUpdate);
            await _context.SaveChangesAsync();
            return new NoContentResult();
        }

        [HttpPost("{id}/request")]
        public async Task<IActionResult> RequestRefund(long id, [FromBody] RequestRefundDTO model)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Unauthorized();
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
                return Unauthorized();
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

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
using Refundeo.Data;
using Refundeo.Data.Models;
using Refundeo.Models.QRCode;
using Refundeo.Models.RefundCase;
using ZXing;
using ZXing.QrCode;

namespace Refundeo.Controllers.User
{
    [Authorize(Roles = RefundeoConstants.ROLE_USER)]
    [Route("/api/user/refundcase")]
    public class UserRefundCaseController : RefundCaseController
    {
        public UserRefundCaseController(RefundeoDbContext context, UserManager<RefundeoUser> userManager) : base(context, userManager)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUserRefundCases()
        {
            var user = await GetCallingUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var refundCases = context.RefundCases
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

            return GenerateRefundCaseDTOResponse(refundCases);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserRefundCaseById(long id)
        {
            var user = await GetCallingUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var refundCase = await context.RefundCases
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

            return GenerateRefundCaseDTOResponse(refundCase);
        }

        [HttpPost("{id}/doc")]
        public async Task<IActionResult> UploadDocumentation(long id, [FromBody] DocementationDTO model)
        {
            var user = await GetCallingUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }
            if (!ModelState.IsValid || string.IsNullOrEmpty(model.Image))
            {
                return BadRequest();
            }


            var refundCaseToUpdate = await context.RefundCases
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
                documentation.Image = ConvertBase64ToByteArray(model.Image);
            }
            catch (System.FormatException)
            {
                return BadRequest("Image should be base64 encoded");
            }

            await context.Documentations.AddAsync(documentation);
            await context.SaveChangesAsync();
            refundCaseToUpdate.Documentation = documentation;
            context.RefundCases.Update(refundCaseToUpdate);
            await context.SaveChangesAsync();
            return new NoContentResult();
        }

        [HttpPost("{id}/refund")]
        public async Task<IActionResult> RequestRefund(long id, [FromBody] RequestRefundDTO model)
        {
            var user = await GetCallingUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var refundCaseToUpdate = await context.RefundCases
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
            context.RefundCases.Update(refundCaseToUpdate);
            await context.SaveChangesAsync();
            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMerchantRefundCase(long id)
        {
            var user = await GetCallingUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var refundCase = await context.RefundCases
            .Include(r => r.CustomerInformation)
            .ThenInclude(i => i.Customer)
            .FirstOrDefaultAsync(r => r.Id == id && r.CustomerInformation.Customer == user);

            if (refundCase == null)
            {
                return NotFound();
            }

            context.RefundCases.Remove(refundCase);
            await context.SaveChangesAsync();
            return new NoContentResult();
        }
    }
}
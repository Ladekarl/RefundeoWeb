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

namespace Refundeo.Controllers.Merchant
{
    [Authorize(Roles = RefundeoConstants.ROLE_MERCHANT)]
    [Route("/api/merchant/refundcase")]
    public class MerchantRefundCaseController : RefundCaseController
    {
        public MerchantRefundCaseController(RefundeoDbContext context, UserManager<RefundeoUser> userManager) : base(context, userManager)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMerchantRefundCases()
        {
            var user = await GetCallingUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var refundCases = await context.RefundCases
            .Include(r => r.Documentation)
            .Include(r => r.MerchantInformation.Merchant)
            .Include(r => r.CustomerInformation.Customer)
            .Where(r => r.MerchantInformation.Merchant.Id == user.Id)
            .ToListAsync();

            if (refundCases == null)
            {
                return NotFound();
            }

            return GenerateRefundCaseDTOResponse(refundCases);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMerchantRefundCaseById(long id)
        {
            var user = await GetCallingUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var refundCase = await context.RefundCases
            .Include(r => r.Documentation)
            .Include(r => r.CustomerInformation.Customer)
            .Include(r => r.MerchantInformation.Merchant)
            .FirstOrDefaultAsync(r => r.Id == id && r.MerchantInformation.Merchant.Id == user.Id);

            if (refundCase == null)
            {
                return NotFound();
            }

            return GenerateRefundCaseDTOResponse(refundCase);
        }

        // Do merchants ever need to create refund cases?
        // [HttpPost]
        // public async Task<IActionResult> CreateMerchantRefundCase([FromBody] CreateRefundCaseDTO model)
        // {
        //     var user = await GetCallingUserAsync();
        //     if (user == null)
        //     {
        //         return Unauthorized();
        //     }

        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest();
        //     }

        //     var merchantInformation = await context.MerchantInformations.FirstOrDefaultAsync(i => i.Merchant.Id == user.Id);
        //     if (merchantInformation == null)
        //     {
        //         return NotFound();
        //     }

        //     var refundCase = new RefundCase
        //     {
        //         Amount = model.Amount,
        //         RefundAmount = model.Amount,
        //         MerchantInformation = merchantInformation
        //     };

        //     var refundCaseResult = await context.RefundCases.AddAsync(refundCase);

        //     await context.SaveChangesAsync();

        //     var qrCode = new QRCode
        //     {
        //         Image = GenerateQRCode(model.QRCodeHeight, model.QRCodeWidth, model.QRCodeMargin, new QRCodePayloadDTO
        //         {
        //             RefundCaseId = refundCase.Id,
        //             MerchantId = user.Id,
        //             RefundAmount = model.Amount
        //         })
        //     };
        //     await context.QRCodes.AddAsync(qrCode);
        //     refundCase.QRCode = qrCode;
        //     context.RefundCases.Update(refundCase);
        //     await context.SaveChangesAsync();

        //     return GenerateRefundCaseDTOResponse(refundCaseResult.Entity);
        // }

        // Do Merchants ever need to update a refund case?

        // [HttpPut("{id}")]
        // public async Task<IActionResult> UpdateMerchantRefundCase(long id, [FromBody] RefundCaseDTO model)
        // {
        //     var user = await GetCallingUserAsync();
        //     if (user == null)
        //     {
        //         return Unauthorized();
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
        //     refundCaseToUpdate.IsRequested = model.IsRequested;

        //     context.RefundCases.Update(refundCaseToUpdate);
        //     await context.SaveChangesAsync();
        //     return new NoContentResult();
        // }

        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptRefund(long id, [FromBody] AcceptRefundCaseDTO model)
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
            .Include(r => r.MerchantInformation.Merchant)
            .FirstOrDefaultAsync(r => r.Id == id && r.MerchantInformation.Merchant.Id == user.Id);

            if (refundCaseToUpdate == null)
            {
                return NotFound();
            }

            refundCaseToUpdate.IsAccepted = model.IsAccepted;
            context.RefundCases.Update(refundCaseToUpdate);
            await context.SaveChangesAsync();
            return new NoContentResult();
        }

        // TODO: Shouldn't this only be an option if the refund case has been processed?

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMerchantRefundCase(long id)
        {
            var user = await GetCallingUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var refundCase = await context.RefundCases
            .Include(r => r.MerchantInformation.Merchant)
            .FirstOrDefaultAsync(r => r.Id == id && r.MerchantInformation.Merchant.Id == user.Id);
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
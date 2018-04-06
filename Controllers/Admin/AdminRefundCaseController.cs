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

namespace Refundeo.Controllers.Admin
{
    [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
    [Route("/api/admin/refundcase")]
    public class AdminRefundCaseController : RefundCaseController
    {
        public AdminRefundCaseController(RefundeoDbContext context, UserManager<RefundeoUser> userManager) : base(context, userManager)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRefundCases()
        {
            var refundCases = context.RefundCases
                .Include(r => r.Customer)
                .Include(r => r.Merchant)
                .Include(r => r.QRCode)
                .Include(r => r.Documentation);

            if (refundCases == null)
            {
                return NotFound();
            }

            return await GenerateRefundCaseDTOResponseAsync(refundCases);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRefundCaseById(long id)
        {
            var refundCase = await context.RefundCases
                .Include(r => r.QRCode)
                .Include(r => r.Customer)
                .Include(r => r.Merchant)
                .Include(r => r.Documentation)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (refundCase == null)
            {
                return NotFound();
            }

            return await GenerateRefundCaseDTOResponseAsync(refundCase);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRefundCase([FromBody] AdminCreateRefundCaseDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var merchant = await userManager.FindByIdAsync(model.MerchantId);

            if (merchant == null)
            {
                return BadRequest("Merchant not found");
            }

            RefundeoUser customer = null;
            if (model.CustomerId != null)
            {
                customer = await userManager.FindByIdAsync(model.CustomerId);

                if (customer == null)
                {
                    return BadRequest("Customer not found");
                }
            }

            var refundCase = new RefundCase
            {
                Amount = model.Amount,
                RefundAmount = model.Amount,
                Merchant = merchant,
                Customer = customer
            };

            var refundCaseResult = await context.RefundCases.AddAsync(refundCase);

            await context.SaveChangesAsync();

            var qrCode = new QRCode
            {
                Image = GenerateQRCode(model.Height, model.Width, model.Margin, new QRCodePayloadDTO
                {
                    RefundCaseId = refundCase.Id,
                    MerchantId = merchant.Id,
                    RefundAmount = model.Amount
                })
            };

            await context.QRCodes.AddAsync(qrCode);
            refundCase.QRCode = qrCode;
            context.RefundCases.Update(refundCase);
            await context.SaveChangesAsync();

            return await GenerateRefundCaseDTOResponseAsync(refundCaseResult.Entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRefundCase(long id, [FromBody] AdminUpdateRefundCaseDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var refundCaseToUpdate = await context.RefundCases
            .Include(r => r.Customer)
            .Include(r => r.Merchant)
            .Include(r => r.Documentation)
            .Include(r => r.QRCode)
            .FirstOrDefaultAsync(r => r.Id == id);

            if (refundCaseToUpdate == null)
            {
                return NotFound();
            }

            refundCaseToUpdate.Amount = model.Amount;
            refundCaseToUpdate.CustomerId = model.CustomerId = model.CustomerId;
            refundCaseToUpdate.MerchantId = model.MerchantId = model.MerchantId;
            refundCaseToUpdate.Documentation.Image = ConvertBase64ToByteArray(model.Documentation);
            refundCaseToUpdate.QRCode.Image = ConvertBase64ToByteArray(model.QRCode);
            refundCaseToUpdate.RefundAmount = model.RefundAmount;

            context.RefundCases.Update(refundCaseToUpdate);
            await context.SaveChangesAsync();
            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRefundCase(long id)
        {
            var refundCase = await context.RefundCases.FirstOrDefaultAsync(r => r.Id == id);
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
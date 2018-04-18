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

namespace Refundeo.Controllers.Admin
{
    [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
    [Route("/api/admin/refundcase")]
    public class AdminRefundCaseController : Controller
    {
        private RefundeoDbContext _context;
        private IRefundCaseService _refundCaseService;
        public AdminRefundCaseController(RefundeoDbContext context, IRefundCaseService refundCaseService)
        {
            _context = context;
            _refundCaseService = refundCaseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRefundCases()
        {
            var refundCases = await _context.RefundCases
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Merchant)
                .Include(r => r.QRCode)
                .Include(r => r.Documentation)
                .ToListAsync();

            if (refundCases == null)
            {
                return NotFound();
            }

            return _refundCaseService.GenerateRefundCaseDTOResponse(refundCases);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRefundCaseById(long id)
        {
            var refundCase = await _context.RefundCases
                .Include(r => r.QRCode)
                .Include(r => r.Documentation)
                .Include(r => r.CustomerInformation)
                .ThenInclude(i => i.Customer)
                .Include(r => r.MerchantInformation)
                .ThenInclude(i => i.Merchant)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (refundCase == null)
            {
                return NotFound();
            }

            return _refundCaseService.GenerateRefundCaseDTOResponse(refundCase);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRefundCase([FromBody] AdminCreateRefundCaseDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var merchantInformation = await _context.MerchantInformations
            .Include(i => i.Merchant)
            .FirstOrDefaultAsync(i => i.Merchant.Id == model.MerchantId);

            if (merchantInformation == null)
            {
                return BadRequest("Merchant not found");
            }

            CustomerInformation customerInformation = null;
            if (model.CustomerId != null)
            {
                customerInformation = await _context.CustomerInformations
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.Customer.Id == model.CustomerId);

                if (customerInformation == null)
                {
                    return BadRequest("Customer not found");

                }
            }

            var refundCase = new RefundCase
            {
                Amount = model.Amount,
                RefundAmount = model.Amount,
                MerchantInformation = merchantInformation,
                CustomerInformation = customerInformation,
                DateCreated = DateTime.UtcNow
            };

            var refundCaseResult = await _context.RefundCases.AddAsync(refundCase);

            await _context.SaveChangesAsync();

            var qrCode = new QRCode
            {
                Image = _refundCaseService.GenerateQRCode(model.QRCodeHeight, model.QRCodeWidth, model.QRCodeMargin, new QRCodePayloadDTO
                {
                    RefundCaseId = refundCase.Id,
                    MerchantId = merchantInformation.Merchant.Id,
                    RefundAmount = model.Amount
                })
            };

            await _context.QRCodes.AddAsync(qrCode);
            refundCase.QRCode = qrCode;
            _context.RefundCases.Update(refundCase);
            await _context.SaveChangesAsync();

            return _refundCaseService.GenerateRefundCaseDTOResponse(refundCaseResult.Entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRefundCase(long id, [FromBody] AdminUpdateRefundCaseDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var refundCaseToUpdate = await _context.RefundCases
            .Include(r => r.CustomerInformation)
            .ThenInclude(i => i.Customer)
            .Include(r => r.MerchantInformation)
            .ThenInclude(i => i.Merchant)
            .Include(r => r.Documentation)
            .Include(r => r.QRCode)
            .FirstOrDefaultAsync(r => r.Id == id);

            if (refundCaseToUpdate == null)
            {
                return NotFound();
            }

            var merchantInformation = await _context.MerchantInformations.FirstOrDefaultAsync(i => i.Merchant.Id == model.MerchantId);
            var customerInformation = await _context.CustomerInformations.FirstOrDefaultAsync(i => i.Customer.Id == model.CustomerId);

            if (merchantInformation == null || customerInformation == null)
            {
                return BadRequest();
            }

            refundCaseToUpdate.Amount = model.Amount;
            refundCaseToUpdate.CustomerInformation = customerInformation;
            refundCaseToUpdate.MerchantInformation = merchantInformation;
            refundCaseToUpdate.IsRequested = model.IsRequested;
            refundCaseToUpdate.IsAccepted = model.IsAccepted;
            refundCaseToUpdate.Documentation.Image = _refundCaseService.ConvertBase64ToByteArray(model.Documentation);
            refundCaseToUpdate.QRCode.Image = _refundCaseService.ConvertBase64ToByteArray(model.QRCode);
            refundCaseToUpdate.RefundAmount = model.RefundAmount;

            _context.RefundCases.Update(refundCaseToUpdate);
            await _context.SaveChangesAsync();
            return new NoContentResult();
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

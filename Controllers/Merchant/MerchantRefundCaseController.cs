using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace Refundeo.Controllers.Merchant
{
    [Authorize(Roles = RefundeoConstants.ROLE_MERCHANT)]
    [Route("/api/merchant/refundcase")]
    public class MerchantRefundCaseController : Controller
    {
        private RefundeoDbContext _context;
        private UserManager<RefundeoUser> _userManager;
        private IRefundCaseService _refundCaseService;
        private IUtilityService _utilityService;
        private IPaginationService<RefundCase> _paginationService;
        public MerchantRefundCaseController(RefundeoDbContext context, UserManager<RefundeoUser> userManager, IRefundCaseService refundCaseService, IUtilityService utilityService, IPaginationService<RefundCase> paginationService)
        {
            _context = context;
            _userManager = userManager;
            _refundCaseService = refundCaseService;
            _utilityService = utilityService;
            _paginationService = paginationService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllMerchantRefundCases()
        {
            var user = await _utilityService.GetCallingUserAsync(Request);

            if (user == null)
            {
                return Unauthorized();
            }

            var refundCases = await _context.RefundCases
            .Include(r => r.Documentation)
            .Include(r => r.MerchantInformation.Merchant)
            .Include(r => r.CustomerInformation.Customer)
            .Where(r => r.MerchantInformation.Merchant.Id == user.Id)
            .ToListAsync();

            if (refundCases == null)
            {
                return NotFound();
            }

            return _refundCaseService.GenerateRefundCaseDTOResponse(refundCases);
        }

        [HttpGet("{first}/{amount}/{sortBy}/{dir}/{filterBy}")]
        public async Task<IActionResult> GetPaginatedMerchantRefundCases(int first, int amount, string sortBy, string dir, string filterBy)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);

            if (user == null)
            {
                return Unauthorized();
            }

            var query = _context.RefundCases
            .Where(r => r.MerchantInformation.Merchant.Id == user.Id);

            query = _paginationService.SortAndFilter(query, sortBy, dir, filterBy);

            var totalRecords = await query
            .AsNoTracking()
            .CountAsync();

            query = query
            .Include(r => r.Documentation)
            .Include(r => r.MerchantInformation.Merchant)
            .Include(r => r.CustomerInformation.Customer);

            var refundCases = await _paginationService
            .Paginate(query, first, amount)
            .AsNoTracking()
            .ToListAsync();

            if (refundCases == null)
            {
                return NotFound();
            }

            var dtos = new List<RefundCaseDTO>();
            foreach (var refundCase in refundCases)
            {
                dtos.Add(_refundCaseService.ConvertRefundCaseToDTO(refundCase));
            }

            return new ObjectResult(new
            {
                totalRecords = totalRecords,
                refundCases = dtos
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMerchantRefundCaseById(long id)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Unauthorized();
            }

            var refundCase = await _context.RefundCases
            .Include(r => r.Documentation)
            .Include(r => r.CustomerInformation.Customer)
            .Include(r => r.MerchantInformation.Merchant)
            .FirstOrDefaultAsync(r => r.Id == id && r.MerchantInformation.Merchant.Id == user.Id);

            if (refundCase == null)
            {
                return NotFound();
            }

            return _refundCaseService.GenerateRefundCaseDTOResponse(refundCase);
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
            .Include(r => r.MerchantInformation.Merchant)
            .FirstOrDefaultAsync(r => r.Id == id && r.MerchantInformation.Merchant.Id == user.Id);

            if (refundCaseToUpdate == null)
            {
                return NotFound();
            }

            refundCaseToUpdate.IsAccepted = model.IsAccepted;
            _context.RefundCases.Update(refundCaseToUpdate);
            await _context.SaveChangesAsync();
            return new NoContentResult();
        }

        // TODO: Shouldn't this only be an option if the refund case has been processed?

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMerchantRefundCase(long id)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Unauthorized();
            }

            var refundCase = await _context.RefundCases
            .Include(r => r.MerchantInformation.Merchant)
            .FirstOrDefaultAsync(r => r.Id == id && r.MerchantInformation.Merchant.Id == user.Id);
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

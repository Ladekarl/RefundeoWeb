using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Controllers.Merchant
{
    [Authorize(Roles = RefundeoConstants.RoleMerchant)]
    [Route("/api/merchant/customerinfo")]
    public class MerchantCustomerInfoController : Controller
    {
        private readonly IUtilityService _utilityService;
        private readonly RefundeoDbContext _context;

        public MerchantCustomerInfoController(RefundeoDbContext context, UserManager<RefundeoUser> userManager,
            IUtilityService utilityService)
        {
            _context = context;
            _utilityService = utilityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerInformation()
        {
            var userId = _utilityService.GetCallingUserId(Request);

            var customerInfos = await _context.MerchantInformations
                .Include(m => m.Merchants)
                .Where(m => m.Merchants.Any(x => x.Id == userId))
                .SelectMany(m => m.RefundCases.Where(c => c.CustomerInformation != null))
                .Select(r => new
                {
                    r.CustomerInformation.Country
                }).ToListAsync();

            if (customerInfos == null)
            {
                return NotFound();
            }

            return new ObjectResult(customerInfos);
        }
    }
}

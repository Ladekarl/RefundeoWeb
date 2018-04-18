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
    [Authorize(Roles = RefundeoConstants.ROLE_MERCHANT)]
    [Route("/api/merchant/customerinfo")]
    public class MerchantCustomerInfoController : Controller
    {
        private UserManager<RefundeoUser> _userManager;
        private IUtilityService _utilityService;
        private RefundeoDbContext _context;

        public MerchantCustomerInfoController(RefundeoDbContext context, UserManager<RefundeoUser> userManager, IUtilityService utilityService)
        {
            _context = context;
            _userManager = userManager;
            _utilityService = utilityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerInformation()
        {
            var userId = _utilityService.GetCallingUserId(Request);

            var customerInfos = await _context.MerchantInformations
            .Where(m => m.Merchant.Id == userId)
            .SelectMany(m => m.RefundCases.Where(c => c.CustomerInformation != null))
            .Select(r => new
            {
                Id = r.CustomerInformation.Id,
                FirstName = r.CustomerInformation.FirstName,
                LastName = r.CustomerInformation.LastName,
                Country = r.CustomerInformation.Country
            }).ToListAsync();

            if (customerInfos == null)
            {
                return NotFound();
            }

            return new ObjectResult(customerInfos);
        }
    }
}

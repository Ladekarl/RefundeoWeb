using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.Account;

namespace Refundeo.Controllers.Merchant {

    [Authorize(Roles = RefundeoConstants.ROLE_MERCHANT)]
    [Route("/api/merchant/customer")]
    public class MerchantCustomerController
    {
        public MerchantCustomerController(RefundeoDbContext context, IConfiguration config, UserManager<RefundeoUser> userManager, SignInManager<RefundeoUser> signManager)
        {
        }
    }
}
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Data;
using Refundeo.Data.Models;

namespace Refundeo.Controllers
{
    public abstract class RefundeoController : Controller
    {
        protected readonly RefundeoDbContext context;
        protected readonly UserManager<RefundeoUser> userManager;
        public RefundeoController(RefundeoDbContext context, UserManager<RefundeoUser> userManager)
        {
            this.userManager = userManager;
            this.context = context;
        }
        protected async Task<RefundeoUser> GetCallingUserAsync()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
            {
                return null;
            }
            return await userManager.FindByIdAsync(userClaim.Value);
        }
    }

    public static class RefundeoConstants
    {
        public const string ROLE_ADMIN = "Admin";
        public const string ROLE_MERCHANT = "Merchant";
        public const string ROLE_USER = "User";
    }
}
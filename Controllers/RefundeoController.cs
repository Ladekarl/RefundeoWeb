using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Data.Models;

namespace Refundeo.Controllers
{
    public abstract class RefundeoController : Controller
    {
        protected readonly UserManager<RefundeoUser> userManager;
        public RefundeoController(UserManager<RefundeoUser> userManager)
        {
            this.userManager = userManager;
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
}
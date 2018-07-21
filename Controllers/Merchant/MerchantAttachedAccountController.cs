using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.Account;
using Refundeo.Core.Services;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Controllers.Merchant
{
    [Authorize(Roles = RefundeoConstants.RoleMerchant)]
    [Route("/api/merchant/attachedaccount")]
    public class MerchantAttachedAccountController : Controller
    {
        private readonly RefundeoDbContext _context;
        private readonly UserManager<RefundeoUser> _userManager;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUtilityService _utilityService;

        public MerchantAttachedAccountController(
            RefundeoDbContext context,
            UserManager<RefundeoUser> userManager,
            IAuthenticationService authenticationService,
            IUtilityService utilityService)
        {
            _context = context;
            _userManager = userManager;
            _authenticationService = authenticationService;
            _utilityService = utilityService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAttachedMerchant([FromBody] CreateAttachedAccountDto model)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);

            if (user == null || model.Username == null || model.Password == null)
            {
                return BadRequest();
            }

            var merchantInformation = await _context.MerchantInformations
                .Include(m => m.Merchants)
                .Where(m => m.Merchants.Any(x => x.Id == user.Id))
                .FirstOrDefaultAsync();

            if (merchantInformation == null)
            {
                return NotFound();
            }

            var attachedUser = new RefundeoUser {UserName = model.Username};
            var createUserResult = await _userManager.CreateAsync(attachedUser, model.Password);

            if (!createUserResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(createUserResult.Errors);
            }

            var addToRoleResult =
                await _userManager.AddToRoleAsync(attachedUser, RefundeoConstants.RoleAttachedMerchant);

            if (!addToRoleResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            merchantInformation.Merchants.Add(attachedUser);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("ChangePassword/{id}")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model, string id)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);

            if (user == null || string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var merchantInformation = await _context.MerchantInformations
                .Include(m => m.Merchants)
                .Where(m => m.Merchants.Any(x => x.Id == user.Id))
                .FirstOrDefaultAsync();

            if (merchantInformation == null)
            {
                return NotFound();
            }

            var attachedUser = merchantInformation.Merchants.FirstOrDefault(m => m.Id == id);

            if (attachedUser == null)
            {
                return NotFound();
            }

            var signInResult =
                await _authenticationService.IsValidUserAndPasswordCombinationAsync(user.UserName, model.OldPassword);
            if (signInResult.Id != SignInId.Success)
            {
                return _utilityService.GenerateBadRequestObjectResult(signInResult.Desc);
            }

            if (model.NewPassword != model.PasswordConfirmation)
            {
                return _utilityService.GenerateBadRequestObjectResult(
                    "New password and password confirmation does not match");
            }

            var changePasswordResult =
                await _userManager.ChangePasswordAsync(attachedUser, model.OldPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(changePasswordResult.Errors.Select(x => x.Description));
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttachedMerchant(string id)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);

            if (user == null || string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var merchantInformation = await _context.MerchantInformations
                .Include(m => m.Merchants)
                .Where(m => m.Merchants.Any(x => x.Id == user.Id))
                .FirstOrDefaultAsync();

            if (merchantInformation == null)
            {
                return NotFound();
            }

            var attachedUser = await _userManager.FindByIdAsync(id);

            if (attachedUser == null)
            {
                return NotFound();
            }

            merchantInformation.Merchants.Remove(attachedUser);

            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(attachedUser);

            if (!result.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(result.Errors);
            }

            return NoContent();
        }
    }
}

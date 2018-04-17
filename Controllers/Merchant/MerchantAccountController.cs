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
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Controllers.Merchant
{
    [Route("/api/merchant/account")]
    public class MerchantAccountController : Controller
    {
        private RefundeoDbContext _context;
        private UserManager<RefundeoUser> _userManager;
        private IUtilityService _utilityService;
        private IAuthenticationService _authenticationService;
        public MerchantAccountController(RefundeoDbContext context, UserManager<RefundeoUser> userManager, IUtilityService utilityService, IAuthenticationService authenticationService)
        {
            _context = context;
            _userManager = userManager;
            _utilityService = utilityService;
            _authenticationService = authenticationService;
        }

        [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
        [HttpGet]
        public async Task<IList<MerchantInformationDTO>> GetAllMerchants()
        {
            var userModels = new List<MerchantInformationDTO>();
            foreach (var u in await _context.MerchantInformations.Include(i => i.Merchant).ToListAsync())
            {
                userModels.Add(_utilityService.ConvertMerchantInformationToDTO(u));
            }
            return userModels;
        }

        [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
        [HttpPost]
        public async Task<IActionResult> RegisterMerchant([FromBody] MerchantRegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = new RefundeoUser { UserName = model.Username };
            var createUserResult = await _userManager.CreateAsync(user, model.Password);

            if (!createUserResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(createUserResult.Errors);
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, RefundeoConstants.ROLE_MERCHANT);

            if (!addToRoleResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            var merchantInformation = new MerchantInformation
            {
                CompanyName = model.CompanyName,
                CVRNumber = model.CVRNumber,
                RefundPercentage = model.RefundPercentage,
                Merchant = user
            };

            await _context.MerchantInformations.AddAsync(merchantInformation);
            await _context.SaveChangesAsync();

            return await _authenticationService.GenerateTokenResultAsync(user);
        }

        [Authorize(Roles = RefundeoConstants.ROLE_MERCHANT)]
        [HttpPut]
        public async Task<IActionResult> ChangeMerchant([FromBody] ChangeMerchantDTO model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return _utilityService.GenerateBadRequestObjectResult("Merchant does not exist");
            }

            var merchantInformation = await _context.MerchantInformations
            .Include(i => i.Merchant)
            .FirstOrDefaultAsync(i => i.Merchant == user);

            if (merchantInformation == null)
            {
                return NotFound();
            }

            user.UserName = model.Username;

            var updateUserResult = await _userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(updateUserResult.Errors);
            }

            merchantInformation.CompanyName = model.CompanyName;
            merchantInformation.CVRNumber = model.CVRNumber;
            merchantInformation.RefundPercentage = model.RefundPercentage;

            _context.MerchantInformations.Update(merchantInformation);
            await _context.SaveChangesAsync();

            return new NoContentResult();
        }

        [Authorize(Roles = RefundeoConstants.ROLE_MERCHANT)]
        [HttpDelete]
        public async Task<IActionResult> DeleteMerchant()
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return _utilityService.GenerateBadRequestObjectResult($"Merchant does not exist");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(result.Errors);
            }

            return new NoContentResult();
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Refundeo.Data;
using Refundeo.Data.Models;
using Refundeo.Models.Account;

namespace Refundeo.Controllers.Merchant
{
    [Route("/api/merchant/account")]
    public class MerchantAccountController : AuthenticationController
    {
        public MerchantAccountController(RefundeoDbContext context, IConfiguration config, UserManager<RefundeoUser> userManager, SignInManager<RefundeoUser> signManager) : base(context, config, userManager, signManager)
        {
        }

        [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
        [HttpGet]
        public async Task<IList<MerchantInformationDTO>> GetAllMerchants()
        {
            var userModels = new List<MerchantInformationDTO>();
            foreach (var u in await context.MerchantInformations.Include(i => i.Merchant).ToListAsync())
            {
                userModels.Add(ConvertMerchantInformationToDTO(u));
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
            var createUserResult = await userManager.CreateAsync(user, model.Password);

            if (!createUserResult.Succeeded)
            {
                return GenerateBadRequestObjectResult(createUserResult.Errors);
            }

            var addToRoleResult = await userManager.AddToRoleAsync(user, RefundeoConstants.ROLE_MERCHANT);

            if (!addToRoleResult.Succeeded)
            {
                return GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            var merchantInformation = new MerchantInformation
            {
                CompanyName = model.CompanyName,
                CVRNumber = model.CVRNumber,
                RefundPercentage = model.RefundPercentage,
                Merchant = user
            };

            await context.MerchantInformations.AddAsync(merchantInformation);
            await context.SaveChangesAsync();

            return await GenerateTokenResultAsync(user);
        }

        [Authorize(Roles = RefundeoConstants.ROLE_MERCHANT)]
        [HttpPut]
        public async Task<IActionResult> ChangeMerchant([FromBody] ChangeMerchantDTO model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            var user = await GetCallingUserAsync();
            if (user == null)
            {
                return GenerateBadRequestObjectResult("Merchant does not exist");
            }

            var merchantInformation = await context.MerchantInformations
            .Include(i => i.Merchant)
            .FirstOrDefaultAsync(i => i.Merchant == user);

            if (merchantInformation == null)
            {
                return NotFound();
            }

            user.UserName = model.Username;

            var updateUserResult = await userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                return GenerateBadRequestObjectResult(updateUserResult.Errors);
            }

            merchantInformation.CompanyName = model.CompanyName;
            merchantInformation.CVRNumber = model.CVRNumber;
            merchantInformation.RefundPercentage = model.RefundPercentage;

            context.MerchantInformations.Update(merchantInformation);
            await context.SaveChangesAsync();

            return new NoContentResult();
        }

        [Authorize(Roles = RefundeoConstants.ROLE_MERCHANT)]
        [HttpDelete]
        public async Task<IActionResult> DeleteMerchant()
        {
            var user = await GetCallingUserAsync();
            if (user == null)
            {
                return GenerateBadRequestObjectResult($"Merchant does not exist");
            }

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return GenerateBadRequestObjectResult(result.Errors);
            }

            return new NoContentResult();
        }
    }
}
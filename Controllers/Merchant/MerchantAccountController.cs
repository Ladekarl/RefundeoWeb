using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Refundeo.Data;
using Refundeo.Data.Models;
using Refundeo.Models.Account;

namespace Refundeo.Controllers.Merchant
{
    [Authorize(Roles = "Merchant")]
    [Route("/api/merchant/account")]
    public class MerchantAccountController : AuthenticationController
    {
        public MerchantAccountController(RefundeoDbContext context, IConfiguration config, UserManager<RefundeoUser> userManager, SignInManager<RefundeoUser> signManager) : base(context, config, userManager, signManager)
        {
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IList<UserDTO>> GetAllMerchants()
        {
            var userModels = new List<UserDTO>();
            foreach (var u in await userManager.GetUsersInRoleAsync("Merchant"))
            {
                userModels.Add(await ConvertRefundeoUserToUserDTOAsync(u));
            }
            return userModels;
        }

        [Authorize(Roles = "Admin")]
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
                GenerateBadRequestObjectResult(createUserResult.Errors);
            }

            var addToRoleResult = await userManager.AddToRoleAsync(user, "Merchant");

            if (!addToRoleResult.Succeeded)
            {
                GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            return await GenerateTokenResultAsync(user);
        }

        [HttpPut]
        public async Task<IActionResult> ChangeMerchant([FromBody] ChangeMerchantDTO model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            var user = await GetCallingUserAsync();
            if (user == null || await userManager.IsInRoleAsync(user, "Merchant"))
            {
                GenerateBadRequestObjectResult("Merchant does not exist");
            }

            user.UserName = model.Username;

            var updateUserResult = await userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                return GenerateBadRequestObjectResult(updateUserResult.Errors);
            }

            return new NoContentResult();
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteMerchant()
        {
            var user = await GetCallingUserAsync();
            if (user == null || !await userManager.IsInRoleAsync(user, "Merchant"))
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
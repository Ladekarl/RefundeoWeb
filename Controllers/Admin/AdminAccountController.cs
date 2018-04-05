using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Refundeo.Data;
using Refundeo.Data.Models;
using Refundeo.Models.Account;

namespace Refundeo.Controllers.Admin
{
    [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
    [Route("/api/admin/account")]
    public class AdminAccountController : AuthenticationController
    {
        public AdminAccountController(RefundeoDbContext context, IConfiguration config, UserManager<RefundeoUser> userManager, SignInManager<RefundeoUser> signManager) : base(context, config, userManager, signManager)
        {
        }

        [HttpGet]
        public async Task<IList<UserDTO>> GetAllAdmins()
        {
            var userModels = new List<UserDTO>();
            foreach (var u in await userManager.GetUsersInRoleAsync("Admin"))
            {
                userModels.Add(new UserDTO
                {
                    Id = u.Id,
                    Username = u.UserName,
                    Roles = await userManager.GetRolesAsync(u)
                });
            }
            return userModels;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterDTO model)
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

            var addToRoleResult = await userManager.AddToRoleAsync(user, "Admin");

            if (!addToRoleResult.Succeeded)
            {
                GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            return await GenerateTokenResultAsync(user);
        }

        [HttpPut]
        public async Task<IActionResult> ChangeAdmin([FromBody] ChangeAdminDTO model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            var user = await GetCallingUserAsync();
            if (user == null || await userManager.IsInRoleAsync(user, "Admin"))
            {
                GenerateBadRequestObjectResult("Admin does not exist");
            }

            user.UserName = model.Username;

            var updateUserResult = await userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                return GenerateBadRequestObjectResult(updateUserResult.Errors);
            }

            return new NoContentResult();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAdmin()
        {
            var user = await GetCallingUserAsync();
            if (user == null || !await userManager.IsInRoleAsync(user, "Admin"))
            {
                return GenerateBadRequestObjectResult($"Admin does not exist");
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
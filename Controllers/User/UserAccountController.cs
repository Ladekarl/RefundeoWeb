using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Refundeo.Data;
using Refundeo.Data.Models;
using Refundeo.Models.Account;

namespace Refundeo.Controllers.User
{
    [Authorize(Roles = RefundeoConstants.ROLE_USER)]
    [Route("/api/user/account")]
    public class UserAccountController : AuthenticationController
    {
        public UserAccountController(RefundeoDbContext context, IConfiguration config, UserManager<RefundeoUser> userManager, SignInManager<RefundeoUser> signManager) : base(context, config, userManager, signManager)
        {
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IList<UserDTO>> GetAllUsers()
        {
            var userModels = new List<UserDTO>();
            foreach (var u in await userManager.GetUsersInRoleAsync("User"))
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

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterDTO model)
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

            var addToRoleResult = await userManager.AddToRoleAsync(user, "User");

            if (!addToRoleResult.Succeeded)
            {
                GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            return await GenerateTokenResultAsync(user);
        }

        [HttpPut]
        public async Task<IActionResult> ChangeUser([FromBody] ChangeUserDTO model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            var user = await GetCallingUserAsync();
            if (user == null || await userManager.IsInRoleAsync(user, "User"))
            {
                GenerateBadRequestObjectResult("User does not exist");
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
        public async Task<IActionResult> DeleteUser()
        {
            var user = await GetCallingUserAsync();
            if (user == null || !await userManager.IsInRoleAsync(user, "User"))
            {
                return GenerateBadRequestObjectResult($"User does not exist");
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
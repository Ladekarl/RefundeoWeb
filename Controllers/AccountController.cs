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

namespace Refundeo.Controllers
{
    [Route("/api/account")]
    public class AccountController : AuthenticationController
    {
        public AccountController(RefundeoDbContext context, IConfiguration config, UserManager<RefundeoUser> userManager, SignInManager<RefundeoUser> signManager) : base(context, config, userManager, signManager)
        {
        }

        [AllowAnonymous]
        [Route("/Token")]
        [HttpPost]
        public async Task<IActionResult> Token([FromBody] UserLoginDTO userLogin)
        {
            var result = await IsValidUserAndPasswordCombinationAsync(userLogin.Username, userLogin.Password);
            if (result.Id != SignInId.SUCCESS)
            {
                return new BadRequestObjectResult(
                    new
                    {
                        error = result.Id,
                        message = result.Desc
                    });
            }

            var user = await userManager.FindByNameAsync(userLogin.Username);

            if (user == null)
            {
                return new NoContentResult();
            }

            return await GenerateTokenResultAsync(user);
        }

        [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
        [HttpGet]
        public async Task<IList<UserDTO>> GetAllAccounts()
        {
            var userModels = new List<UserDTO>();
            foreach (var u in await userManager.Users.ToListAsync())
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

        [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
        [HttpGet("{id}", Name = "GetAccount")]
        public async Task<IActionResult> GetAccountById(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return new ObjectResult(new
            {
                Id = user.Id,
                Username = user.UserName,
                Roles = userManager.GetRolesAsync(user)
            });
        }

        [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
        [HttpPost]
        public async Task<IActionResult> RegisterAccount([FromBody] AccountRegisterDTO model)
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

            var addToRoleResult = await userManager.AddToRolesAsync(user, model.Roles);

            if (!addToRoleResult.Succeeded)
            {
                GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            return await GenerateTokenResultAsync(user);
        }

        [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
        [HttpPut]
        public async Task<IActionResult> ChangeAccount([FromBody] ChangeAccountDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                GenerateBadRequestObjectResult("Account does not exist");
            }

            user.UserName = model.Username;

            var updateUserResult = await userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                return GenerateBadRequestObjectResult(updateUserResult.Errors);
            }

            var updateRolesResult = await UpdateRolesAsync(user, model.Roles);
            if (!updateRolesResult.Succeeded)
            {
                return GenerateBadRequestObjectResult(updateRolesResult.Errors);
            }

            return new NoContentResult();
        }

        [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return GenerateBadRequestObjectResult($"Account with id={id} does not exist");
            }

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return GenerateBadRequestObjectResult(result.Errors);
            }

            return new NoContentResult();
        }

        [Authorize]
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            var user = await GetCallingUserAsync();
            if (user == null)
            {
                return GenerateBadRequestObjectResult("No user found");
            }

            var signInResult = await IsValidUserAndPasswordCombinationAsync(user.UserName, model.OldPassword);
            if (signInResult.Id != SignInId.SUCCESS)
            {
                return GenerateBadRequestObjectResult(signInResult.Desc);
            }

            if (model.NewPassword != model.PasswordConfirmation)
            {
                return GenerateBadRequestObjectResult("New password and password confirmation does not match");
            }

            var changePasswordResult = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                return GenerateBadRequestObjectResult(changePasswordResult.Errors);
            }

            return new NoContentResult();
        }
    }
}
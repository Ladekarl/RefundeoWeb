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

namespace Refundeo.Controllers
{
    [Route("/api/account")]
    public class AccountController : Controller
    {
        private IAuthenticationService _authenticationService;
        private UserManager<RefundeoUser> _userManager;
        private IUtilityService _utilityService;
        public AccountController(UserManager<RefundeoUser> userManager, IAuthenticationService authenticationService, IUtilityService utilityService)
        {
            _authenticationService = authenticationService;
            _userManager = userManager;
            _utilityService = utilityService;
        }

        [AllowAnonymous]
        [Route("/Token")]
        [HttpPost]
        public async Task<IActionResult> Token([FromBody] UserLoginDTO userLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _authenticationService.IsValidUserAndPasswordCombinationAsync(userLogin.Username, userLogin.Password);
            if (result.Id != SignInId.SUCCESS)
            {
                return new BadRequestObjectResult(
                    new
                    {
                        error = result.Id,
                        message = result.Desc
                    });
            }

            var user = await _userManager.FindByNameAsync(userLogin.Username);

            if (user == null)
            {
                return new NoContentResult();
            }

            return await _authenticationService.GenerateTokenResultAsync(user);
        }

        [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
        [HttpGet]
        public async Task<IList<UserDTO>> GetAllAccounts()
        {
            var userModels = new List<UserDTO>();
            foreach (var u in await _userManager.Users.ToListAsync())
            {
                userModels.Add(await _utilityService.ConvertRefundeoUserToUserDTOAsync(u));
            }
            return userModels;
        }

        [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return new ObjectResult(await _utilityService.ConvertRefundeoUserToUserDTOAsync(user));
        }

        [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return _utilityService.GenerateBadRequestObjectResult($"Account with id={id} does not exist");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(result.Errors);
            }

            return new NoContentResult();
        }

        [Authorize]
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var signInResult = await _authenticationService.IsValidUserAndPasswordCombinationAsync(user.UserName, model.OldPassword);
            if (signInResult.Id != SignInId.SUCCESS)
            {
                return _utilityService.GenerateBadRequestObjectResult(signInResult.Desc);
            }

            if (model.NewPassword != model.PasswordConfirmation)
            {
                return _utilityService.GenerateBadRequestObjectResult("New password and password confirmation does not match");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(changePasswordResult.Errors);
            }

            return new NoContentResult();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.Account;
using Refundeo.Core.Services.Interfaces;
using IAuthenticationService = Refundeo.Core.Services.Interfaces.IAuthenticationService;

namespace Refundeo.Controllers
{
    [Route("/api/account")]
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly UserManager<RefundeoUser> _userManager;
        private readonly IUtilityService _utilityService;
        private readonly RefundeoDbContext _context;

        public AccountController(UserManager<RefundeoUser> userManager, IAuthenticationService authenticationService,
            IUtilityService utilityService, RefundeoDbContext context)
        {
            _authenticationService = authenticationService;
            _userManager = userManager;
            _utilityService = utilityService;
            _context = context;
        }

        [AllowAnonymous]
        [Route("/Token/Facebook")]
        [HttpPost]
        public async Task<IActionResult> LoginFacebook([FromBody] LoginFacebookDto model)
        {
            if (string.IsNullOrEmpty(model.AccessToken))
            {
                return BadRequest("Invalid OAuth access token");
            }

            var fbUser = await VerifyFacebookAccessToken(model.AccessToken);

            if (fbUser == null)
            {
                return BadRequest("Invalid OAuth access token");
            }

            var user = await _userManager.FindByNameAsync(fbUser.Email);

            var shouldCreateRefreshToken = model.Scopes != null && model.Scopes.Contains("offline_access");

            if (user != null)
            {
                string refreshToken = null;
                if (shouldCreateRefreshToken)
                {
                    refreshToken = await _authenticationService.CreateAndSaveRefreshTokenAsync(user);
                }

                return await _authenticationService.GenerateTokenResultAsync(user, refreshToken);
            }

            var newUser = new RefundeoUser {UserName = fbUser.Email};

            var customerInformation = new CustomerInformation
            {
                FirstName = fbUser.FirstName,
                LastName = fbUser.LastName,
                Country = fbUser.Location.Location.Country,
                IsOauth = true
            };
            return await _authenticationService.RegisterUserAsync(newUser,
                _authenticationService.GenerateRandomPassword(), customerInformation, shouldCreateRefreshToken);
        }

        [AllowAnonymous]
        [Route("/Token")]
        [HttpPost]
        public async Task<IActionResult> Token([FromBody] UserLoginDto userLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (userLogin.GrantType != null && userLogin.GrantType == "refresh_token")
            {
                if (userLogin.RefreshToken == null)
                {
                    return BadRequest();
                }

                var grantUser = await _context.Users.Where(u => u.RefreshToken == userLogin.RefreshToken)
                    .FirstOrDefaultAsync();

                if (grantUser == null)
                {
                    return NotFound();
                }

                return await _authenticationService.GenerateTokenResultAsync(grantUser);
            }

            var result =
                await _authenticationService.IsValidUserAndPasswordCombinationAsync(userLogin.Username,
                    userLogin.Password);

            if (result.Id != SignInId.Success)
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
                return NotFound();
            }

            if (userLogin.Scopes == null || !userLogin.Scopes.Contains("offline_access"))
                return await _authenticationService.GenerateTokenResultAsync(user);

            var refreshToken = await _authenticationService.CreateAndSaveRefreshTokenAsync(user);

            return await _authenticationService.GenerateTokenResultAsync(user, refreshToken);
        }

        [Authorize(Roles = RefundeoConstants.RoleAdmin)]
        [HttpGet]
        public async Task<IList<UserDto>> GetAllAccounts()
        {
            var userModels = new List<UserDto>();
            foreach (var u in await _userManager.Users.ToListAsync())
            {
                userModels.Add(await _utilityService.ConvertRefundeoUserToUserDtoAsync(u));
            }

            return userModels;
        }

        [Authorize(Roles = RefundeoConstants.RoleAdmin)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return new ObjectResult(await _utilityService.ConvertRefundeoUserToUserDtoAsync(user));
        }

        [Authorize(Roles = RefundeoConstants.RoleAdmin)]
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
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
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
                await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(changePasswordResult.Errors);
            }

            return NoContent();
        }

        private static async Task<FacebookUserViewModel> VerifyFacebookAccessToken(string accessToken)
        {
            var path =
                "https://graph.facebook.com/me?fields=id,first_name,last_name,email,gender,birthday,location{location}&access_token=" +
                accessToken;
            var client = new HttpClient();
            var uri = new Uri(path);
            var response = await client.GetAsync(uri);

            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FacebookUserViewModel>(content);
        }
    }
}

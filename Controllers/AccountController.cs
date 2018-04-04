using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Refundeo.Data.Models;
using Refundeo.Models.Account;

namespace Refundeo.Controllers
{
    [Route("/api/[controller]")]
    public class AccountController : Controller
    {
        public IConfiguration Configuration { get; set; }
        private readonly UserManager<RefundeoUser> _userManager;
        private readonly SignInManager<RefundeoUser> _signManager;
        public AccountController(IConfiguration config, UserManager<RefundeoUser> userManager, SignInManager<RefundeoUser> signManager)
        {
            Configuration = config;
            _userManager = userManager;
            _signManager = signManager;
        }

        #region /Token/Token

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

            var user = await _userManager.FindByNameAsync(userLogin.Username);

            if (user == null)
            {
                return new NoContentResult();
            }

            return await GenerateTokenResultAsync(user);
        }

        #endregion

        #region /Account

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IList<UserDTO>> GetAllAccounts()
        {
            var userModels = new List<UserDTO>();
            foreach (var u in await _userManager.Users.ToListAsync())
            {
                userModels.Add(new UserDTO
                {
                    Id = u.Id,
                    Username = u.UserName,
                    Roles = await _userManager.GetRolesAsync(u)
                });
            }
            return userModels;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}", Name = "GetAccount")]
        public async Task<IActionResult> GetAccountById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return new ObjectResult(new
            {
                Id = user.Id,
                Username = user.UserName,
                Roles = _userManager.GetRolesAsync(user)
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RegisterAccount([FromBody] AccountRegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = new RefundeoUser { UserName = model.Username };
            var createUserResult = await _userManager.CreateAsync(user, model.Password);

            if (!createUserResult.Succeeded)
            {
                GenerateBadRequestObjectResult(createUserResult.Errors);
            }

            var addToRoleResult = await _userManager.AddToRolesAsync(user, model.Roles);

            if (!addToRoleResult.Succeeded)
            {
                GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            return await GenerateTokenResultAsync(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> ChangeAccount([FromBody] ChangeAccountDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                GenerateBadRequestObjectResult("Account does not exist");
            }

            user.UserName = model.Username;

            var updateUserResult = await _userManager.UpdateAsync(user);
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

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return GenerateBadRequestObjectResult($"Account with id={id} does not exist");
            }

            var result = await _userManager.DeleteAsync(user);
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

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                return GenerateBadRequestObjectResult(changePasswordResult.Errors);
            }

            return new NoContentResult();
        }

        #endregion

        #region /Account/User

        [Authorize(Roles = "Admin")]
        [HttpGet("User")]
        public async Task<IList<UserDTO>> GetAllUsers()
        {
            var userModels = new List<UserDTO>();
            foreach (var u in await _userManager.GetUsersInRoleAsync("User"))
            {
                userModels.Add(new UserDTO
                {
                    Id = u.Id,
                    Username = u.UserName,
                    Roles = await _userManager.GetRolesAsync(u)
                });
            }
            return userModels;
        }

        [AllowAnonymous]
        [HttpPost("User")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = new RefundeoUser { UserName = model.Username };
            var createUserResult = await _userManager.CreateAsync(user, model.Password);

            if (!createUserResult.Succeeded)
            {
                GenerateBadRequestObjectResult(createUserResult.Errors);
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, "User");

            if (!addToRoleResult.Succeeded)
            {
                GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            return await GenerateTokenResultAsync(user);
        }

        [Authorize(Roles = "User")]
        [HttpPut("User")]
        public async Task<IActionResult> ChangeUser([FromBody] ChangeUserDTO model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            
            var user = await GetCallingUserAsync();
            if (user == null || await _userManager.IsInRoleAsync(user, "User"))
            {
                GenerateBadRequestObjectResult("User does not exist");
            }

            user.UserName = model.Username;

            var updateUserResult = await _userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                return GenerateBadRequestObjectResult(updateUserResult.Errors);
            }

            return new NoContentResult();
        }

        [Authorize(Roles = "User")]
        [HttpDelete("User")]
        public async Task<IActionResult> DeleteUser()
        {
            var user = await GetCallingUserAsync();
            if (user == null || !await _userManager.IsInRoleAsync(user, "User"))
            {
                return GenerateBadRequestObjectResult($"User does not exist");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return GenerateBadRequestObjectResult(result.Errors);
            }

            return new NoContentResult();
        }

        #endregion

        #region /Account/Merchant

        [Authorize(Roles = "Admin")]
        [HttpGet("Merchant")]
        public async Task<IList<UserDTO>> GetAllMerchants()
        {
            var userModels = new List<UserDTO>();
            foreach (var u in await _userManager.GetUsersInRoleAsync("Merchant"))
            {
                userModels.Add(new UserDTO
                {
                    Id = u.Id,
                    Username = u.UserName,
                    Roles = await _userManager.GetRolesAsync(u)
                });
            }
            return userModels;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Merchant")]
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
                GenerateBadRequestObjectResult(createUserResult.Errors);
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, "Merchant");

            if (!addToRoleResult.Succeeded)
            {
                GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            return await GenerateTokenResultAsync(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("Merchant")]
        public async Task<IActionResult> ChangeMerchant([FromBody] ChangeMerchantDTO model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            
            var user = await GetCallingUserAsync();
            if (user == null || await _userManager.IsInRoleAsync(user, "Merchant"))
            {
                GenerateBadRequestObjectResult("Merchant does not exist");
            }

            user.UserName = model.Username;

            var updateUserResult = await _userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                return GenerateBadRequestObjectResult(updateUserResult.Errors);
            }

            return new NoContentResult();
        }

        [Authorize(Roles = "Merchant")]
        [HttpDelete("Merchant")]
        public async Task<IActionResult> DeleteMerchant()
        {
            var user = await GetCallingUserAsync();
            if (user == null || !await _userManager.IsInRoleAsync(user, "Merchant"))
            {
                return GenerateBadRequestObjectResult($"Merchant does not exist");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return GenerateBadRequestObjectResult(result.Errors);
            }

            return new NoContentResult();
        }

        #endregion

        #region /Account/Admin

        [Authorize(Roles = "Admin")]
        [HttpGet("Admin")]
        public async Task<IList<UserDTO>> GetAllAdmins()
        {
            var userModels = new List<UserDTO>();
            foreach (var u in await _userManager.GetUsersInRoleAsync("Admin"))
            {
                userModels.Add(new UserDTO
                {
                    Id = u.Id,
                    Username = u.UserName,
                    Roles = await _userManager.GetRolesAsync(u)
                });
            }
            return userModels;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = new RefundeoUser { UserName = model.Username };
            var createUserResult = await _userManager.CreateAsync(user, model.Password);

            if (!createUserResult.Succeeded)
            {
                GenerateBadRequestObjectResult(createUserResult.Errors);
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, "Admin");

            if (!addToRoleResult.Succeeded)
            {
                GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            return await GenerateTokenResultAsync(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("Admin")]
        public async Task<IActionResult> ChangeAdmin([FromBody] ChangeAdminDTO model)
        {
             if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            
            var user = await GetCallingUserAsync();
            if (user == null || await _userManager.IsInRoleAsync(user, "Admin"))
            {
                GenerateBadRequestObjectResult("Admin does not exist");
            }

            user.UserName = model.Username;

            var updateUserResult = await _userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                return GenerateBadRequestObjectResult(updateUserResult.Errors);
            }

            return new NoContentResult();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("Admin")]
        public async Task<IActionResult> DeleteAdmin()
        {
            var user = await GetCallingUserAsync();
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return GenerateBadRequestObjectResult($"Admin does not exist");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return GenerateBadRequestObjectResult(result.Errors);
            }

            return new NoContentResult();
        }

        #endregion

        #region Helpers Methods

        private async Task<RefundeoUser> GetCallingUserAsync()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
            {
                return null;
            }
            return await _userManager.FindByIdAsync(userClaim.Value);
        }

        private async Task<IdentityResult> UpdateRolesAsync(RefundeoUser user, ICollection<string> roles)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesAdded = roles.Except(currentRoles);
            var rolesRemoved = currentRoles.Except(roles);

            if (rolesAdded.Count() > 0)
            {
                var result = await _userManager.AddToRolesAsync(user, rolesAdded);
                if (!result.Succeeded)
                {
                    return result;
                }
            }

            if (rolesRemoved.Count() > 0)
            {
                var result = await _userManager.RemoveFromRolesAsync(user, rolesRemoved);
                if (!result.Succeeded)
                {
                    return result;
                }
            }

            return IdentityResult.Success;
        }

        private ObjectResult GenerateBadRequestObjectResult(params string[] errors)
        {
            return GenerateBadRequestObjectResult(errors.ToList());
        }

        private ObjectResult GenerateBadRequestObjectResult(IEnumerable errors)
        {
            return new BadRequestObjectResult(new
            {
                errors = errors
            });
        }

        private async Task<ObjectResult> GenerateTokenResultAsync(RefundeoUser user)
        {
            var token = await GenerateTokenAsync(user);

            return new ObjectResult(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                id = user.Id,
                username = user.UserName,
                roles = await _userManager.GetRolesAsync(user)
            });
        }

        private async Task<SignInResult> IsValidUserAndPasswordCombinationAsync(string username, string password)
        {
            if (String.IsNullOrEmpty(username))
            {
                return new SignInResult.NoUsername();
            }
            if (String.IsNullOrEmpty(password))
            {
                return new SignInResult.NoPassword();
            }

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return new SignInResult.UserDoesNotExist();
            }

            var isValid = await _signManager.UserManager.CheckPasswordAsync(user, password);

            if (!isValid)
            {
                return new SignInResult.WrongPassword();
            }
            return new SignInResult.Success();
        }

        private async Task<JwtSecurityToken> GenerateTokenAsync(RefundeoUser user)
        {
            var claims = GenerateClaims(user);

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return new JwtSecurityToken(
                new JwtHeader(
                    new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSecurityKey"])),
                        SecurityAlgorithms.HmacSha256)),
                        new JwtPayload(claims));
        }

        private List<Claim> GenerateClaims(RefundeoUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, Configuration["ValidAudience"]),
                new Claim(JwtRegisteredClaimNames.Iss, Configuration["ValidIssuer"])
            };
            return claims;
        }

        #endregion
    }

    #region Helper Classes

    public abstract class SignInResult
    {
        public abstract SignInId Id { get; }
        public abstract string Desc { get; }

        public class WrongPassword : SignInResult
        {
            public override SignInId Id { get { return SignInId.WRONG_PASSWORD; } }
            public override string Desc { get { return "Wrong password"; } }
        }

        public class UserDoesNotExist : SignInResult
        {
            public override SignInId Id { get { return SignInId.USER_DOES_NOT_EXIST; } }
            public override string Desc { get { return "User does not exist"; } }
        }

        public class NoPassword : SignInResult
        {
            public override SignInId Id { get { return SignInId.NO_PASSWORD; } }
            public override string Desc { get { return "No password provided"; } }
        }

        public class NoUsername : SignInResult
        {
            public override SignInId Id { get { return SignInId.NO_USERNAME; } }
            public override string Desc { get { return "No username provided"; } }
        }

        public class Success : SignInResult
        {
            public override SignInId Id { get { return SignInId.SUCCESS; } }
            public override string Desc { get { return "Success"; } }
        }
    }

    public enum SignInId
    {
        SUCCESS = 1,
        WRONG_PASSWORD = -1,
        USER_DOES_NOT_EXIST = -2,
        NO_PASSWORD = -3,
        NO_USERNAME = -4
    }

    #endregion
}
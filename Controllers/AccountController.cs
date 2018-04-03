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
using Refundeo.Models;

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

        [AllowAnonymous]
        [Route("/Token")]
        [HttpPost]
        public async Task<IActionResult> Token([FromBody] UserLogin userLogin)
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

            return await GenerateTokenResultAsync(user, true);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IList<UserModel>> GetAll()
        {
            var userModels = new List<UserModel>();
            foreach (var u in await _userManager.Users.ToListAsync())
            {
                userModels.Add(new UserModel
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
        public async Task<IActionResult> GetById(string id)
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

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegister model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = new RefundeoUser { UserName = model.Username };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return new BadRequestObjectResult(new
                {
                    success = result.Succeeded,
                    errors = result.Errors
                });
            }

            return await GenerateTokenResultAsync(user, result.Succeeded);
        }

        private async Task<ObjectResult> GenerateTokenResultAsync(RefundeoUser user, bool success)
        {
            var token = await GenerateTokenAsync(user);

            return new ObjectResult(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
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
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, Configuration["ValidAudience"]),
                new Claim(JwtRegisteredClaimNames.Iss, Configuration["ValidIssuer"])
            };
            return claims;
        }
    }

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
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
        public async Task<IActionResult> Login([FromBody] UserLogin userLogin)
        {
            if (!ModelState.IsValid | !await IsValidUserAndPasswordCombination(userLogin.Username, userLogin.Password))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByNameAsync(userLogin.Username);

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
                success = success,
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }

        private async Task<bool> IsValidUserAndPasswordCombination(string username, string password)
        {
            if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
                return false;
            var user = await _userManager.FindByNameAsync(username);
            return await _signManager.UserManager.CheckPasswordAsync(user, password);
        }

        private async Task<JwtSecurityToken> GenerateTokenAsync(RefundeoUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, Configuration["ValidAudience"]),
                new Claim(JwtRegisteredClaimNames.Iss, Configuration["ValidIssuer"])
            };

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
    }
}
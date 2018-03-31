using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Refundeo.Models;

namespace Refundeo.Controllers
{
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

        [Route("/Token")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserLogin userLogin)
        {
            if (!ModelState.IsValid | !await IsValidUserAndPasswordCombination(userLogin.Username, userLogin.Password))
            {
                return BadRequest();
            }

            return GenerateTokenResult(userLogin.Username, true);
        }

        [Route("/api/[controller]")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegister model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = new RefundeoUser { UserName = model.Username, IsMerchant = model.IsMerchant };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return new BadRequestObjectResult(new
                {
                    success = result.Succeeded,
                    errors = result.Errors
                });
            }

            return GenerateTokenResult(user.UserName, result.Succeeded);
        }

        private ObjectResult GenerateTokenResult(string username, bool success)
        {
            var token = GenerateToken(username);

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

        private JwtSecurityToken GenerateToken(string username)
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
            };

            return new JwtSecurityToken(
                new JwtHeader(
                    new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSecurityKey"])),
                        SecurityAlgorithms.HmacSha256)),
                        new JwtPayload(claims));
        }
    }
}
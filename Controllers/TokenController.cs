using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
    public class TokenController : Controller
    {
        public IConfiguration Configuration { get; set; }
        private readonly UserManager<RefundeoUser> _userManager;
        private readonly SignInManager<RefundeoUser> _signInManager;
        public TokenController(IConfiguration config, UserManager<RefundeoUser> userManager, SignInManager<RefundeoUser> signInManager) 
        {
            Configuration = config;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Route("/Token")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserLogin userLogin) 
        {
            if (await IsValidUserAndPasswordCombination(userLogin.Username, userLogin.Password)) 
            {
                var token = GenerateToken(userLogin.Username);
                return new ObjectResult(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return BadRequest();
        }

        private async Task<bool> IsValidUserAndPasswordCombination(string username, string password)
        {
            if(String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password)) 
                return false;
            var user = await _userManager.FindByNameAsync(username);
            return await _signInManager.UserManager.CheckPasswordAsync(user, password);
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
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes("j#{5s!!uk^K!Vuq<")),
                        SecurityAlgorithms.HmacSha256)),
                        new JwtPayload(claims));
        }   
    }
}
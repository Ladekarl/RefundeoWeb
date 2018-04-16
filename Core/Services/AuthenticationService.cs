using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Core.Services
{
    public class AuthenticationService: IAuthenticationService
    {
        public IConfiguration Configuration { get; set; }
        private readonly SignInManager<RefundeoUser> _signManager;
        private readonly UserManager<RefundeoUser> _userManager;
        public AuthenticationService(IConfiguration configuration, UserManager<RefundeoUser> userManager, SignInManager<RefundeoUser> signManager)
        {
            Configuration = configuration;
            this._signManager = signManager;
            this._userManager = userManager;
        }

        public async Task<IdentityResult> UpdateRolesAsync(RefundeoUser user, ICollection<string> roles)
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

        public async Task<ObjectResult> GenerateTokenResultAsync(RefundeoUser user)
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

        public async Task<Helpers.SignInResult> IsValidUserAndPasswordCombinationAsync(string username, string password)
        {
            if (String.IsNullOrEmpty(username))
            {
                return new Helpers.SignInResult.NoUsername();
            }
            if (String.IsNullOrEmpty(password))
            {
                return new Helpers.SignInResult.NoPassword();
            }

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return new Helpers.SignInResult.UserDoesNotExist();
            }

            var isValid = await _signManager.UserManager.CheckPasswordAsync(user, password);

            if (!isValid)
            {
                return new Helpers.SignInResult.WrongPassword();
            }
            return new Helpers.SignInResult.Success();
        }

        public async Task<JwtSecurityToken> GenerateTokenAsync(RefundeoUser user)
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

        public ICollection<Claim> GenerateClaims(RefundeoUser user)
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
    }
}
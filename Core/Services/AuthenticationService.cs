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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Core.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        public IConfiguration Configuration { get; set; }
        private readonly SignInManager<RefundeoUser> _signManager;
        private readonly UserManager<RefundeoUser> _userManager;
        private readonly RefundeoDbContext _context;
        public AuthenticationService(IConfiguration configuration, UserManager<RefundeoUser> userManager, SignInManager<RefundeoUser> signManager, RefundeoDbContext context)
        {
            Configuration = configuration;
            this._signManager = signManager;
            this._userManager = userManager;
            this._context = context;
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

        public async Task<ObjectResult> GenerateTokenResultAsync(RefundeoUser user, string refreshToken)
        {
            var token = await GenerateTokenAsync(user);

            var merchantInformation = await _context.MerchantInformations.Where(m => m.Merchant.Id == user.Id).FirstOrDefaultAsync();
            var customerInformation = await _context.CustomerInformations.Where(c => c.Customer.Id == user.Id).FirstOrDefaultAsync();

            if (merchantInformation != null)
            {
                return await GenerateMerchantObjectResultAsync(token, user, merchantInformation, refreshToken);
            }
            else if (customerInformation != null)
            {
                return await GenerateCustomertObjectResultAsync(token, user, customerInformation, refreshToken);
            }
            else
            {
                return new ObjectResult(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    id = user.Id,
                    username = user.UserName,
                    refreshToken = refreshToken,
                    roles = await _userManager.GetRolesAsync(user)
                });
            }
        }

        private async Task<ObjectResult> GenerateMerchantObjectResultAsync(JwtSecurityToken token, RefundeoUser user, MerchantInformation merchantInformation, string refreshToken)
        {
            return new ObjectResult(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                id = user.Id,
                username = user.UserName,
                companyName = merchantInformation.CompanyName,
                cvrNumber = merchantInformation.CVRNumber,
                refundPercentage = merchantInformation.RefundPercentage,
                roles = await _userManager.GetRolesAsync(user),
                refreshToken = refreshToken
            });
        }

        private async Task<ObjectResult> GenerateCustomertObjectResultAsync(JwtSecurityToken token, RefundeoUser user, CustomerInformation customerInformation, string refreshToken)
        {
            return new ObjectResult(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                id = user.Id,
                username = user.UserName,
                firstName = customerInformation.FirstName,
                lastName = customerInformation.LastName,
                country = customerInformation.Country,
                bankAccountNumber = customerInformation.BankAccountNumber,
                bankRegNumber = customerInformation.BankRegNumber,
                roles = await _userManager.GetRolesAsync(user),
                refreshToken = refreshToken
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

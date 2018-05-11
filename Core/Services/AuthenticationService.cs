using System;
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
using SignInResult = Refundeo.Core.Helpers.SignInResult;

namespace Refundeo.Core.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private IConfiguration Configuration { get; }
        private readonly SignInManager<RefundeoUser> _signManager;
        private readonly UserManager<RefundeoUser> _userManager;
        private readonly RefundeoDbContext _context;
        private readonly IUtilityService _utilityService;

        public AuthenticationService(IConfiguration configuration, UserManager<RefundeoUser> userManager,
            SignInManager<RefundeoUser> signManager, RefundeoDbContext context, IUtilityService utilityService)
        {
            Configuration = configuration;
            _signManager = signManager;
            _userManager = userManager;
            _context = context;
            _utilityService = utilityService;
        }

        public async Task<IdentityResult> UpdateRolesAsync(RefundeoUser user, ICollection<string> roles)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesAdded = roles.Except(currentRoles);
            var rolesRemoved = currentRoles.Except(roles);

            var rolesAddedList = rolesAdded.ToList();
            if (rolesAddedList.Any())
            {
                var result = await _userManager.AddToRolesAsync(user, rolesAddedList);
                if (!result.Succeeded)
                {
                    return result;
                }
            }

            var rolesRemovedList = rolesRemoved.ToList();
            if (!rolesRemovedList.Any()) return IdentityResult.Success;
            {
                var result = await _userManager.RemoveFromRolesAsync(user, rolesRemovedList);
                if (!result.Succeeded)
                {
                    return result;
                }
            }

            return IdentityResult.Success;
        }

        public async Task<ObjectResult> GenerateTokenResultAsync(RefundeoUser user, string refreshToken = null)
        {
            var token = await GenerateTokenAsync(user);

            var merchantInformation = await _context.MerchantInformations.Where(m => m.Merchant.Id == user.Id)
                .FirstOrDefaultAsync();
            var customerInformation = await _context.CustomerInformations.Where(c => c.Customer.Id == user.Id)
                .FirstOrDefaultAsync();

            if (merchantInformation != null)
            {
                return await GenerateMerchantObjectResultAsync(token, user, merchantInformation, refreshToken);
            }

            if (customerInformation != null)
            {
                return await GenerateCustomertObjectResultAsync(token, user, customerInformation, refreshToken);
            }

            return new ObjectResult(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                id = user.Id,
                username = user.UserName,
                refreshToken,
                roles = await _userManager.GetRolesAsync(user)
            });
        }

        private async Task<ObjectResult> GenerateMerchantObjectResultAsync(JwtSecurityToken token, RefundeoUser user,
            MerchantInformation merchantInformation, string refreshToken)
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
                refreshToken
            });
        }

        private async Task<ObjectResult> GenerateCustomertObjectResultAsync(JwtSecurityToken token, RefundeoUser user,
            CustomerInformation customerInformation, string refreshToken)
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
                refreshToken
            });
        }

        public async Task<SignInResult> IsValidUserAndPasswordCombinationAsync(string username, string password)
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
                new Claim(JwtRegisteredClaimNames.Exp,
                    new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, Configuration["ValidAudience"]),
                new Claim(JwtRegisteredClaimNames.Iss, Configuration["ValidIssuer"])
            };
            return claims;
        }

        public string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null)
                opts = new PasswordOptions
                {
                    RequiredLength = 8,
                    RequiredUniqueChars = 4,
                    RequireDigit = false,
                    RequireLowercase = true,
                    RequireNonAlphanumeric = false,
                    RequireUppercase = false
                };

            string[] randomChars =
            {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ", // uppercase
                "abcdefghijkmnopqrstuvwxyz", // lowercase
                "0123456789", // digits
                "!@$?_-" // non-alphanumeric
            };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count;
                i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars;
                i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        public async Task<ObjectResult> RegisterUserAsync(RefundeoUser user, string password,
            CustomerInformation customerInformation, bool shouldCreateRefreshToken = false)
        {
            var createUserResult = await _userManager.CreateAsync(user, password);

            if (!createUserResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(createUserResult.Errors);
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, RefundeoConstants.RoleUser);

            await _context.SaveChangesAsync();

            if (!addToRoleResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            await _context.CustomerInformations.AddAsync(customerInformation);
            await _context.SaveChangesAsync();

            string refreshToken = null;
            if (shouldCreateRefreshToken)
            {
                refreshToken = await CreateAndSaveRefreshTokenAsync(user);
            }

            return await GenerateTokenResultAsync(user, refreshToken);
        }

        public async Task<string> CreateAndSaveRefreshTokenAsync(RefundeoUser user)
        {
            var refreshToken = Guid.NewGuid() + user.UserName;
            user.RefreshToken = refreshToken;
            await _userManager.UpdateAsync(user);
            return refreshToken;
        }
    }
}

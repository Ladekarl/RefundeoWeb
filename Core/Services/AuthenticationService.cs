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
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.Account;
using Refundeo.Core.Models.QRCode;
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
        private readonly IBlobStorageService _blobStorageService;
        private readonly IOptions<StorageAccountOptions> _optionsAccessor;
        private readonly IRefundCaseService _refundCaseService;

        public AuthenticationService(IConfiguration configuration, UserManager<RefundeoUser> userManager,
            SignInManager<RefundeoUser> signManager, RefundeoDbContext context, IUtilityService utilityService,
            IBlobStorageService blobStorageService, IOptions<StorageAccountOptions> optionsAccessor,
            IRefundCaseService refundCaseService)
        {
            Configuration = configuration;
            _signManager = signManager;
            _userManager = userManager;
            _context = context;
            _utilityService = utilityService;
            _blobStorageService = blobStorageService;
            _optionsAccessor = optionsAccessor;
            _refundCaseService = refundCaseService;
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

            var isUser = await _userManager.IsInRoleAsync(user, RefundeoConstants.RoleUser);

            if (isUser)
            {
                var customerInformation = await _context.CustomerInformations
                    .Include(c => c.Address)
                    .Include(c => c.Customer)
                    .Where(c => c.Customer.Id == user.Id)
                    .FirstOrDefaultAsync();


                if (customerInformation != null)
                {
                    return await GenerateCustomerObjectResultAsync(token, user, customerInformation, refreshToken);
                }
            }

            return await GenerateMerchantObjectResultAsync(token, user, refreshToken);
        }

        private async Task<ObjectResult> GenerateMerchantObjectResultAsync(SecurityToken token, RefundeoUser user,
            string refreshToken)
        {
            return new ObjectResult(new MerchantDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                Id = user.Id,
                Username = user.UserName,
                RefreshToken = refreshToken,
                Roles = await _userManager.GetRolesAsync(user)
            });
        }

        private async Task<ObjectResult> GenerateCustomerObjectResultAsync(SecurityToken token, RefundeoUser user,
            CustomerInformation customerInformation, string refreshToken)
        {
            return new ObjectResult(new CustomerDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                Id = user.Id,
                Username = user.UserName,
                FirstName = customerInformation.FirstName,
                LastName = customerInformation.LastName,
                Country = customerInformation.Country,
                AccountNumber = customerInformation.AccountNumber,
                Swift = customerInformation.Swift,
                IsOauth = customerInformation.IsOauth,
                AcceptedTermsOfService = customerInformation.AcceptedTermsOfService,
                AcceptedPrivacyPolicy = customerInformation.AcceptedPrivacyPolicy,
                PrivacyPolicyVersion = customerInformation.PrivacyPolicyVersion,
                TermsOfServiceVersion = customerInformation.TermsOfServiceVersion,
                Roles = await _userManager.GetRolesAsync(user),
                RefreshToken = refreshToken,
                Email = customerInformation.Email,
                Phone = customerInformation.Phone,
                Passport = customerInformation.Passport,
                AddressCity = customerInformation.Address?.City,
                AddressCountry = customerInformation.Address?.Country,
                AddressStreetName = customerInformation.Address?.StreetName,
                AddressStreetNumber = customerInformation.Address?.StreetNumber,
                AddressPostalCode = customerInformation.Address?.PostalCode,
                DateCreated = customerInformation.DateCreated,
                QRCode = await _utilityService.ConvertBlobPathToBase64Async(customerInformation.QRCode)
            });
        }

        public async Task<SignInResult> IsValidUserAndPasswordCombinationAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new SignInResult.NoUsername();
            }

            if (string.IsNullOrEmpty(password))
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

            var rand = new Random(Environment.TickCount);
            var chars = new List<char>();

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

            for (var i = chars.Count;
                i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars;
                i++)
            {
                var rcs = randomChars[rand.Next(0, randomChars.Length)];
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

            if (!addToRoleResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            customerInformation.Customer = user;

            var qrCode = _utilityService.GenerateQrCode(200, 200, 5, new QRCodeUserId
            {
                UserId = user.Id
            });

            var logoContainerName = _optionsAccessor.Value.QrCodesContainerNameOption;
            customerInformation.QRCode = await _blobStorageService.UploadAsync(logoContainerName,
                $"{user.Id}", _utilityService.ConvertByteArrayToBase64(qrCode),
                "image/png");

            await _context.Addresses.AddAsync(customerInformation.Address);
            await _context.CustomerInformations.AddAsync(customerInformation);
            await _context.SaveChangesAsync();

            string refreshToken = null;
            if (shouldCreateRefreshToken)
            {
                refreshToken = await CreateAndSaveRefreshTokenAsync(user);
            }

            return await GenerateTokenResultAsync(user, refreshToken);
        }

        public async Task<IdentityResult> DeleteUserAsync(RefundeoUser user)
        {
            CustomerInformation customerInformation = null;
            MerchantInformation merchantInformation = null;

            if (await _userManager.IsInRoleAsync(user, RefundeoConstants.RoleUser))
            {
                customerInformation = await _context.CustomerInformations
                    .Where(x => x.Customer.Id == user.Id)
                    .Include(x => x.Address)
                    .Include(x => x.RefundCases)
                    .SingleOrDefaultAsync();
            }

            if (await _userManager.IsInRoleAsync(user, RefundeoConstants.RoleMerchant))
            {
                merchantInformation = await _context.MerchantInformations
                    .Where(x => x.Merchants.Any(m => m.Id == user.Id))
                    .Include(x => x.Address)
                    .Include(x => x.Merchants)
                    .Include(x => x.Location)
                    .Include(x => x.MerchantInformationTags)
                    .Include(x => x.OpeningHours)
                    .Include(x => x.RefundCases)
                    .SingleOrDefaultAsync();
            }

            if (customerInformation != null)
            {
                await DeleteCustomerAsync(customerInformation);
            }

            if (merchantInformation != null)
            {
                var merchantsToDelete = merchantInformation.Merchants
                    .Where(m => m.Id != user.Id)
                    .ToList();

                foreach (var attachedMerchant in merchantsToDelete)
                {
                    await _userManager.DeleteAsync(attachedMerchant);
                }

                await _context.SaveChangesAsync();

                await DeleteMerchantAsync(merchantInformation);
            }

            return await _userManager.DeleteAsync(user);
        }

        private async Task DeleteCustomerAsync(CustomerInformation customerInformation)
        {
            if (!string.IsNullOrEmpty(customerInformation.QRCode))
                await _blobStorageService.DeleteAsync(new Uri(customerInformation.QRCode));

            if (customerInformation.Address != null)
                _context.Addresses.Remove(customerInformation.Address);

            await _refundCaseService.DeleteRefundCasesAsync(customerInformation.RefundCases);

            _context.CustomerInformations.Remove(customerInformation);
            await _context.SaveChangesAsync();
        }

        private async Task DeleteMerchantAsync(MerchantInformation merchantInformation)
        {
            if (!string.IsNullOrEmpty(merchantInformation.Banner))
                await _blobStorageService.DeleteAsync(new Uri(merchantInformation.Banner));
            if (!string.IsNullOrEmpty(merchantInformation.Logo))
                await _blobStorageService.DeleteAsync(new Uri(merchantInformation.Logo));

            if (merchantInformation.Address != null)
                _context.Remove(merchantInformation.Address);

            if (merchantInformation.Location != null)
                _context.Remove(merchantInformation.Location);

            _context.MerchantInformationTags.RemoveRange(merchantInformation.MerchantInformationTags);

            _context.OpeningHours.RemoveRange(merchantInformation.OpeningHours);

            await _refundCaseService.DeleteRefundCasesAsync(merchantInformation.RefundCases);

            _context.MerchantInformations.Remove(merchantInformation);
            await _context.SaveChangesAsync();
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

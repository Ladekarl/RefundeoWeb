using System.Collections;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Models.Account;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Core.Services
{
    public class UtilityService : IUtilityService
    {
        private readonly UserManager<RefundeoUser> _userManager;
        private readonly RefundeoDbContext _context;

        public UtilityService(RefundeoDbContext context, UserManager<RefundeoUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<RefundeoUser> GetCallingUserAsync(HttpRequest request)
        {
            var userId = GetCallingUserId(request);
            if (userId == null)
            {
                return null;
            }

            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<RefundeoUser> GetCallingUserFullAsync(HttpRequest request)
        {
            var userId = GetCallingUserId(request);
            if (userId == null)
            {
                return null;
            }

            return await _context.Users
                .Include(u => u.MerchantInformation)
                .ThenInclude(m => m.Address)
                .Include(u => u.MerchantInformation)
                .ThenInclude(m => m.Location)
                .Include(u => u.CustomerInformation)
                .ThenInclude(c => c.Address)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public string GetCallingUserId(HttpRequest request)
        {
            var userClaim = request.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            return userClaim?.Value;
        }

        public async Task<UserDto> ConvertRefundeoUserToUserDtoAsync(RefundeoUser refundeoUser)
        {
            UserDto userDto = null;
            if (refundeoUser != null)
            {
                userDto = new UserDto
                {
                    Id = refundeoUser.Id,
                    Username = refundeoUser.UserName,
                    Roles = await _userManager.GetRolesAsync(refundeoUser)
                };
            }

            return userDto;
        }

        public CustomerInformationDto ConvertCustomerInformationToDto(CustomerInformation info)
        {
            CustomerInformationDto dto = null;
            if (info != null)
            {
                dto = new CustomerInformationDto
                {
                    Id = info.Customer?.Id,
                    Username = info.Customer?.UserName,
                    FirstName = info.FirstName,
                    LastName = info.LastName,
                    Country = info.Country,
                    AcceptedPrivacyPolicy = info.AcceptedPrivacyPolicy,
                    AcceptedTermsOfService = info.AcceptedTermsOfService,
                    PrivacyPolicy = info.PrivacyPolicy,
                    TermsOfService = info.TermsOfService,
                    IsOauth = info.IsOauth,
                    Email = info.Email,
                    Swift = info.Swift,
                    AddressCity = info.Address?.City,
                    AddressCountry = info.Address?.Country,
                    AddressStreetName = info.Address?.StreetName,
                    AddressStreetNumber = info.Address?.StreetNumber,
                    Passport = info.Passport
                };
            }

            return dto;
        }

        public MerchantInformationDto ConvertMerchantInformationToDto(MerchantInformation info)
        {
            MerchantInformationDto dto = null;
            if (info != null)
            {
                dto = new MerchantInformationDto
                {
                    Id = info.Merchant?.Id,
                    CompanyName = info.CompanyName,
                    CvrNumber = info.CVRNumber,
                    RefundPercentage = info.RefundPercentage,
                    AddressCity = info.Address?.City,
                    AddressCountry = info.Address?.Country,
                    AddressStreetName = info.Address?.StreetName,
                    AddressStreetNumber = info.Address?.StreetNumber,
                    Latitude = info.Location?.Latitude,
                    Longitude = info.Location?.Longitude
                };
            }

            return dto;
        }

        public ObjectResult GenerateBadRequestObjectResult(params string[] errors)
        {
            return GenerateBadRequestObjectResult(errors.ToList());
        }

        public ObjectResult GenerateBadRequestObjectResult(IEnumerable errors)
        {
            return new BadRequestObjectResult(new
            {
                errors
            });
        }
    }
}

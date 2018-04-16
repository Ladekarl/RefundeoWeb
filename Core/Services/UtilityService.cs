using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Models.Account;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Core.Services
{
    public class UtilityService : IUtilityService
    {
        private readonly UserManager<RefundeoUser> _userManager;
        public UtilityService(UserManager<RefundeoUser> userManager)
        {
            this._userManager = userManager;

        }

        public async Task<RefundeoUser> GetCallingUserAsync(HttpRequest request)
        {
            var userClaim = request.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
            {
                return null;
            }
            return await _userManager.FindByIdAsync(userClaim.Value);
        }

        public async Task<UserDTO> ConvertRefundeoUserToUserDTOAsync(RefundeoUser refundeoUser)
        {
            UserDTO userDTO = null;
            if (refundeoUser != null)
            {
                userDTO = new UserDTO
                {
                    Id = refundeoUser.Id,
                    Username = refundeoUser.UserName,
                    Roles = await _userManager.GetRolesAsync(refundeoUser)
                };
            }
            return userDTO;
        }

        public CustomerInformationDTO ConvertCustomerInformationToDTO(CustomerInformation info)
        {
            CustomerInformationDTO dto = null;
            if (info != null)
            {
                dto = new CustomerInformationDTO
                {
                    Id = info.Customer?.Id,
                    Username = info.Customer?.UserName,
                    Firstname = info.FirstName,
                    Lastname = info.LastName,
                    Country = info.Country
                };
            }
            return dto;
        }

        public MerchantInformationDTO ConvertMerchantInformationToDTO(MerchantInformation info)
        {
            MerchantInformationDTO dto = null;
            if (info != null)
            {
                dto = new MerchantInformationDTO
                {
                    Id = info.Merchant?.Id,
                    CompanyName = info.CompanyName,
                    CVRNumber = info.CVRNumber,
                    RefundPercentage = info.RefundPercentage
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
                errors = errors
            });
        }
    }
}
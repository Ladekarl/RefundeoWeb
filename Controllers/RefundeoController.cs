using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Data;
using Refundeo.Data.Models;
using Refundeo.Models.Account;

namespace Refundeo.Controllers
{
    public abstract class RefundeoController : Controller
    {
        protected readonly RefundeoDbContext context;
        protected readonly UserManager<RefundeoUser> userManager;
        public RefundeoController(RefundeoDbContext context, UserManager<RefundeoUser> userManager)
        {
            this.userManager = userManager;
            this.context = context;
        }

        protected async Task<RefundeoUser> GetCallingUserAsync()
        {
            var userClaim = Request.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
            {
                return null;
            }
            return await userManager.FindByIdAsync(userClaim.Value);
        }

        protected async Task<UserDTO> ConvertRefundeoUserToUserDTOAsync(RefundeoUser refundeoUser)
        {
            UserDTO userDTO = null;
            if (refundeoUser != null)
            {
                userDTO = new UserDTO
                {
                    Id = refundeoUser.Id,
                    Username = refundeoUser.UserName,
                    Roles = await userManager.GetRolesAsync(refundeoUser)
                };
            }
            return userDTO;
        }

        protected CustomerInformationDTO ConvertCustomerInformationToDTO(CustomerInformation info)
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

        protected MerchantInformationDTO ConvertMerchantInformationToDTO(MerchantInformation info)
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

        protected ObjectResult GenerateBadRequestObjectResult(params string[] errors)
        {
            return GenerateBadRequestObjectResult(errors.ToList());
        }

        protected ObjectResult GenerateBadRequestObjectResult(IEnumerable errors)
        {
            return new BadRequestObjectResult(new
            {
                errors = errors
            });
        }
    }

    public static class RefundeoConstants
    {
        public const string ROLE_ADMIN = "Admin";
        public const string ROLE_MERCHANT = "Merchant";
        public const string ROLE_USER = "User";
    }
}
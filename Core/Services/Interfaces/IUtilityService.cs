using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Models.Account;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IUtilityService
    {
        string GetCallingUserId(HttpRequest request);
        Task<RefundeoUser> GetCallingUserAsync(HttpRequest request);
        Task<RefundeoUser> GetCallingUserFullAsync(HttpRequest request);
        Task<UserDTO> ConvertRefundeoUserToUserDTOAsync(RefundeoUser refundeoUser);
        CustomerInformationDTO ConvertCustomerInformationToDTO(CustomerInformation info);
        MerchantInformationDTO ConvertMerchantInformationToDTO(MerchantInformation info);
        ObjectResult GenerateBadRequestObjectResult(params string[] errors);
        ObjectResult GenerateBadRequestObjectResult(IEnumerable errors);
    }
}

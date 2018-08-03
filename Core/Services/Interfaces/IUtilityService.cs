using System.Collections;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Models.Account;
using Refundeo.Core.Models.QRCode;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IUtilityService
    {
        string GetCallingUserId(HttpRequest request);
        Task<RefundeoUser> GetCallingUserAsync(HttpRequest request);
        Task<UserDto> ConvertRefundeoUserToUserDtoAsync(RefundeoUser refundeoUser);
        Task<CustomerInformationDto> ConvertCustomerInformationToDtoAsync(CustomerInformation info);
        CustomerInformationSimpleDto ConvertCustomerInformationToSimpleDto(CustomerInformation info);
        Task<MerchantInformationDto> ConvertMerchantInformationToDtoAsync(MerchantInformation info);
        MerchantInformationSimpleDto ConvertMerchantInformationToSimpleDto(MerchantInformation info);
        ObjectResult GenerateBadRequestObjectResult(params string[] errors);
        ObjectResult GenerateBadRequestObjectResult(IEnumerable errors);
        string ConvertByteArrayToBase64(byte[] ba);
        byte[] ConvertBase64ToByteArray(string base64String);
        Task<string> ConvertBlobPathToBase64Async(string path);
        byte[] GenerateQrCode(int height, int width, int margin, QRCodeRefundCaseDto payload);
        byte[] GenerateQrCode(int height, int width, int margin, QRCodeUserId payload);
    }
}

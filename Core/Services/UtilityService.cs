using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models;
using Refundeo.Core.Models.Account;
using Refundeo.Core.Models.QRCode;
using Refundeo.Core.Services.Interfaces;
using ZXing;
using ZXing.QrCode;

namespace Refundeo.Core.Services
{
    public class UtilityService : IUtilityService
    {
        private readonly UserManager<RefundeoUser> _userManager;
        private readonly IBlobStorageService _blobStorageService;

        public UtilityService(UserManager<RefundeoUser> userManager,
            IBlobStorageService blobStorageService)
        {
            _userManager = userManager;
            _blobStorageService = blobStorageService;
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

        public async Task<CustomerInformationDto> ConvertCustomerInformationToDtoAsync(CustomerInformation info)
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
                    PrivacyPolicyVersion = info.PrivacyPolicyVersion,
                    TermsOfServiceVersion = info.TermsOfServiceVersion,
                    IsOauth = info.IsOauth,
                    Email = info.Email,
                    Phone = info.Phone,
                    AccountNumber = info.AccountNumber,
                    Swift = info.Swift,
                    AddressCity = info.Address?.City,
                    AddressCountry = info.Address?.Country,
                    AddressStreetName = info.Address?.StreetName,
                    AddressPostalCode = info.Address?.PostalCode,
                    AddressStreetNumber = info.Address?.StreetNumber,
                    Passport = info.Passport,
                    DateCreated = info.DateCreated,
                    QRCode = await ConvertBlobPathToBase64Async(info.QRCode),
                };
            }

            return dto;
        }

        public CustomerInformationSimpleDto ConvertCustomerInformationToSimpleDto(CustomerInformation info)
        {
            CustomerInformationSimpleDto dto = null;
            if (info != null)
            {
                dto = new CustomerInformationSimpleDto
                {
                    Id = info.Customer?.Id,
                    FirstName = info.FirstName,
                    LastName = info.LastName,
                    Country = info.Country,
                    Email = info.Email,
                    Phone = info.Phone
                };
            }

            return dto;
        }

        public MerchantInformationSimpleDto ConvertMerchantInformationToSimpleDto(MerchantInformation info)
        {
            if (info == null) return null;

            var dto = new MerchantInformationSimpleDto
            {
                CompanyName = info.CompanyName,
                CvrNumber = info.CVRNumber,
                VatRate = info.VATRate,
                PriceLevel = info.PriceLevel,
                FeePoints = info.FeePoints?.Select(f => new FeePointDto
                {
                    AdminFee = f.AdminFee,
                    MerchantFee = f.MerchantFee,
                    End = f.End,
                    Start = f.Start,
                    RefundPercentage = f.RefundPercentage
                }).ToList(),
                AdminEmail = info.AdminEmail,
                AddressCity = info.Address?.City,
                AddressCountry = info.Address?.Country,
                AddressStreetName = info.Address?.StreetName,
                AddressStreetNumber = info.Address?.StreetNumber,
                AddressPostalCode = info.Address?.PostalCode,
                Latitude = info.Location?.Latitude,
                Longitude = info.Location?.Longitude,
                DateCreated = info.DateCreated,
                Description = info.Description,
                OpeningHours =
                    info.OpeningHours?.Select(o =>
                        new OpeningHoursDto
                        {
                            Open = o.Open,
                            Close = o.Close,
                            Day = o.Day
                        }).ToList(),
                Tags = info.MerchantInformationTags?.Select(m => m.Tag.Key).ToList(),
                VatNumber = info.VATNumber,
                ContactEmail = info.ContactEmail,
                ContactPhone = info.ContactPhone,
                Currency = info.Currency,
                Banner = info.Banner,
                Logo = info.Logo
            };

            if (info.City == null) return dto;

            dto.City = new CityDto
            {
                GooglePlaceId = info.City.GooglePlaceId,
                Image = info.City.Image,
                Name = info.City.Name
            };

            if (info.Location == null) return dto;

            dto.Latitude = info.Location.Latitude;
            dto.Longitude = info.Location.Longitude;

            return dto;
        }

        public MerchantInformationRestrictedDto ConvertMerchantInformationToRestrictedDto(MerchantInformation info)
        {
            if (info == null) return null;

            var refundPercentages = info.FeePoints.Select(feePoint => feePoint.RefundPercentage).ToList();

            var dto = new MerchantInformationRestrictedDto
            {
                CompanyName = info.CompanyName,
                CvrNumber = info.CVRNumber,
                VatRate = info.VATRate,
                PriceLevel = info.PriceLevel,
                AddressCity = info.Address?.City,
                AddressCountry = info.Address?.Country,
                AddressStreetName = info.Address?.StreetName,
                AddressStreetNumber = info.Address?.StreetNumber,
                AddressPostalCode = info.Address?.PostalCode,
                Latitude = info.Location?.Latitude,
                Longitude = info.Location?.Longitude,
                DateCreated = info.DateCreated,
                Description = info.Description,
                OpeningHours =
                    info.OpeningHours?.Select(o =>
                        new OpeningHoursDto {Open = o.Open, Close = o.Close, Day = o.Day}).ToList(),
                Tags = info.MerchantInformationTags?.Select(m => m.Tag.Key).ToList(),
                VatNumber = info.VATNumber,
                ContactEmail = info.ContactEmail,
                ContactPhone = info.ContactPhone,
                Currency = info.Currency,
                Banner = info.Banner,
                Logo = info.Logo
            };

            if (info.City != null)
            {
                dto.City = new CityDto
                {
                    GooglePlaceId = info.City.GooglePlaceId,
                    Image = info.City.Image,
                    Name = info.City.Name
                };

                if (info.Location != null)
                {
                    dto.Latitude = info.Location.Latitude;
                    dto.Longitude = info.Location.Longitude;
                }
            }

            if (refundPercentages.Any())
            {
                dto.RefundPercentage = refundPercentages.Min();
            }

            return dto;
        }

        public async Task<MerchantInformationDto> ConvertMerchantInformationToDtoAsync(MerchantInformation info)
        {
            if (info == null) return null;

            RefundeoUser mainMerchant = null;

            foreach (var merchant in info.Merchants)
            {
                if (!await _userManager.IsInRoleAsync(merchant, RefundeoConstants.RoleMerchant)) continue;
                mainMerchant = merchant;
                break;
            }

            var dto = new MerchantInformationDto
            {
                Id = mainMerchant?.Id,
                Username = mainMerchant?.UserName,
                CompanyName = info.CompanyName,
                CvrNumber = info.CVRNumber,
                FeePoints = info.FeePoints?.Select(f => new FeePointDto
                {
                    End = f.End,
                    Start = f.Start,
                    RefundPercentage = f.RefundPercentage,
                    AdminFee = f.AdminFee,
                    MerchantFee = f.MerchantFee
                }).ToList(),
                PriceLevel = info.PriceLevel,
                VatRate = info.VATRate,
                AddressCity = info.Address?.City,
                AddressCountry = info.Address?.Country,
                AddressStreetName = info.Address?.StreetName,
                AddressStreetNumber = info.Address?.StreetNumber,
                AddressPostalCode = info.Address?.PostalCode,
                Latitude = info.Location?.Latitude,
                Longitude = info.Location?.Longitude,
                DateCreated = info.DateCreated,
                Description = info.Description,
                AdminEmail = info.AdminEmail,
                OpeningHours =
                    info.OpeningHours?.Select(o =>
                        new OpeningHoursDto {Open = o.Open, Close = o.Close, Day = o.Day}).ToList(),
                Tags = info.MerchantInformationTags?.Select(m => m.Tag.Key).ToList(),
                VatNumber = info.VATNumber,
                ContactEmail = info.ContactEmail,
                ContactPhone = info.ContactPhone,
                Currency = info.Currency,
                AttachedAccounts = info.Merchants
                    .Where(x => mainMerchant != null && x.Id != mainMerchant.Id)
                    .Select(x => new AttachedAccountDto
                    {
                        Id = x.Id,
                        Username = x.UserName
                    }).ToList(),
                Banner = info.Banner,
                Logo = info.Logo
            };

            if (info.City == null) return dto;
            dto.City = new CityDto
            {
                GooglePlaceId = info.City.GooglePlaceId,
                Image = info.City.Image,
                Name = info.City.Name
            };

            if (info.Location == null) return dto;
            dto.Latitude = info.Location.Latitude;
            dto.Longitude = info.Location.Longitude;

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

        public string ConvertByteArrayToBase64(byte[] ba)
        {
            return ba != null ? Convert.ToBase64String(ba) : null;
        }

        public byte[] ConvertBase64ToByteArray(string base64String)
        {
            byte[] ba = null;
            if (base64String != null)
            {
                ba = Convert.FromBase64String(base64String);
            }

            return ba;
        }

        public async Task<string> ConvertBlobPathToBase64Async(string path)
        {
            string image = null;
            if (path != null)
            {
                var imageBa = await _blobStorageService.DownloadFromPathAsync(path);
                image = ConvertByteArrayToBase64(imageBa);
            }

            return image;
        }

        public byte[] GenerateQrCode(int height, int width, int margin, QRCodeUserId payload)
        {
            return GenerateQrCode(height, width, margin, JsonConvert.SerializeObject(payload.UserId));
        }

        public byte[] GenerateQrCode(int height, int width, int margin, QRCodeRefundCaseDto payload)
        {
            return GenerateQrCode(height, width, margin, JsonConvert.SerializeObject(payload.RefundCaseId));
        }

        private byte[] GenerateQrCode(int height, int width, int margin, string payload)
        {
            var qrCodeWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width,
                    Margin = margin
                }
            };
            var pixelData = qrCodeWriter.Write(payload);
            byte[] image;
            using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            using (var ms = new MemoryStream())
            {
                var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0,
                        pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                image = ms.ToArray();
            }

            return image;
        }
    }
}

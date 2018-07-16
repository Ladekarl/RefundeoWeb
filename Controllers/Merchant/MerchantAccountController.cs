using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.Account;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Controllers.Merchant
{
    [Route("/api/merchant/account")]
    public class MerchantAccountController : Controller
    {
        private readonly RefundeoDbContext _context;
        private readonly UserManager<RefundeoUser> _userManager;
        private readonly IUtilityService _utilityService;
        private readonly IOptions<StorageAccountOptions> _optionsAccessor;
        private readonly IBlobStorageService _blobStorageService;

        private readonly IAuthenticationService _authenticationService;

        public MerchantAccountController(RefundeoDbContext context, UserManager<RefundeoUser> userManager,
            IUtilityService utilityService, IAuthenticationService authenticationService,
            IOptions<StorageAccountOptions> optionsAccessor, IBlobStorageService blobStorageService)
        {
            _context = context;
            _userManager = userManager;
            _utilityService = utilityService;
            _authenticationService = authenticationService;
            _optionsAccessor = optionsAccessor;
            _blobStorageService = blobStorageService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IList<MerchantInformationDto>> GetAllMerchants()
        {
            var userModels = new List<MerchantInformationDto>();
            foreach (var u in await _context.MerchantInformations
                .Include(i => i.Merchant)
                .Include(i => i.Address)
                .Include(i => i.Location)
                .Include(i => i.OpeningHours)
                .Include(i => i.MerchantInformationTags)
                .ThenInclude(i => i.Tag)
                .ToListAsync())
            {
                userModels.Add(await _utilityService.ConvertMerchantInformationToDtoAsync(u));
            }

            return userModels;
        }

        [Authorize(Roles = RefundeoConstants.RoleAdmin)]
        [HttpPost]
        public async Task<IActionResult> RegisterMerchant([FromBody] MerchantRegisterDto model)
        {
            if (!ModelState.IsValid || model.Username == null || model.Password == null)
            {
                return BadRequest();
            }

            var user = new RefundeoUser {UserName = model.Username};
            var createUserResult = await _userManager.CreateAsync(user, model.Password);

            if (!createUserResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(createUserResult.Errors);
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, RefundeoConstants.RoleMerchant);

            if (!addToRoleResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            var address = new Address
            {
                City = model.AddressCity,
                Country = model.AddressCountry,
                StreetName = model.AddressStreetName,
                StreetNumber = model.AddressStreetNumber,
                PostalCode = model.AddressPostalCode,
            };

            await _context.Addresses.AddAsync(address);

            var location = new Location
            {
                Latitude = model.Latitude,
                Longitude = model.Longitude
            };

            await _context.Locations.AddAsync(location);

            foreach (var openingHoursModel in model.OpeningHours)
            {
                var openingHours = new OpeningHours
                {
                    Close = openingHoursModel.Close,
                    Day = openingHoursModel.Day,
                    Open = openingHoursModel.Open
                };
                await _context.OpeningHours.AddAsync(openingHours);
            }

            var merchantInformation = new MerchantInformation
            {
                CompanyName = model.CompanyName,
                CVRNumber = model.CvrNumber,
                RefundPercentage = model.RefundPercentage,
                Merchant = user,
                Location = location,
                Address = address,
                Description = model.Description,
                ContactEmail = model.ContactEmail,
                ContactPhone = model.ContactPhone,
                VATNumber = model.VatNumber,
                Currency = model.Currency
            };

            await _context.MerchantInformations.AddAsync(merchantInformation);

            foreach (var tagModel in model.Tags)
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == tagModel);
                if (tag != null)
                {
                    var merchantInformationTag = new MerchantInformationTag
                    {
                        MerchantInformation = merchantInformation,
                        Tag = tag
                    };
                    _context.MerchantInformationTags.Add(merchantInformationTag);

                    merchantInformation.MerchantInformationTags.Add(merchantInformationTag);
                    tag.MerchantInformationTags.Add(merchantInformationTag);
                }
            }

            await _context.SaveChangesAsync();

            if (model.Logo != null || model.Banner != null)
            {
                if (model.Logo != null)
                {
                    var logoContainerName = _optionsAccessor.Value.MerchantLogosContainerNameOption;
                    merchantInformation.Logo = await _blobStorageService.UploadAsync(logoContainerName,
                        $"{merchantInformation.CompanyName}-{merchantInformation.Id}-logo", model.Logo,
                        "image/png");
                }

                if (model.Banner != null)
                {
                    var bannerContainerName = _optionsAccessor.Value.MerchantBannersContainerNameOption;
                    merchantInformation.Banner = await _blobStorageService.UploadAsync(bannerContainerName,
                        $"{merchantInformation.CompanyName}-{merchantInformation.Id}-banner", model.Banner,
                        "image/png");
                }

                _context.MerchantInformations.Update(merchantInformation);
                await _context.SaveChangesAsync();
            }

            return await _authenticationService.GenerateTokenResultAsync(user);
        }

        [Authorize(Roles = RefundeoConstants.RoleMerchant)]
        [HttpPut]
        public async Task<IActionResult> ChangeMerchant([FromBody] ChangeMerchantDto model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return _utilityService.GenerateBadRequestObjectResult("Merchant does not exist");
            }

            var merchantInformation = await _context.MerchantInformations
                .Include(i => i.Merchant)
                .Include(m => m.Address)
                .Include(m => m.Location)
                .Include(m => m.OpeningHours)
                .Include(m => m.MerchantInformationTags)
                .ThenInclude(m => m.Tag)
                .FirstOrDefaultAsync(i => i.Merchant == user);

            if (merchantInformation == null)
            {
                return NotFound();
            }

            foreach (var openingHoursModel in model.OpeningHours)
            {
                var existingOpeningHours =
                    merchantInformation
                        .OpeningHours
                        .FirstOrDefault(o => o.Day == openingHoursModel.Day);
                if (existingOpeningHours == null)
                {
                    var openingHours = new OpeningHours
                    {
                        Close = openingHoursModel.Close,
                        Day = openingHoursModel.Day,
                        Open = openingHoursModel.Open
                    };
                    _context.OpeningHours.Add(openingHours);
                }
                else
                {
                    existingOpeningHours.Close = openingHoursModel.Close;
                    existingOpeningHours.Open = openingHoursModel.Open;
                    _context.OpeningHours.Update(existingOpeningHours);
                }
            }

            merchantInformation.CompanyName = model.CompanyName;
            merchantInformation.CVRNumber = model.CvrNumber;
            merchantInformation.RefundPercentage = model.RefundPercentage;
            merchantInformation.Address.StreetName = model.AddressStreetName;
            merchantInformation.Address.Country = model.AddressCountry;
            merchantInformation.Address.City = model.AddressCity;
            merchantInformation.Address.StreetNumber = model.AddressStreetNumber;
            merchantInformation.Address.PostalCode = model.AddressPostalCode;
            merchantInformation.Location.Latitude = model.Latitude;
            merchantInformation.Location.Longitude = model.Longitude;
            merchantInformation.Description = model.Description;
            merchantInformation.VATNumber = model.VatNumber;
            merchantInformation.ContactEmail = model.ContactEmail;
            merchantInformation.ContactPhone = model.ContactPhone;
            merchantInformation.Currency = model.Currency;

            _context.MerchantInformations.Update(merchantInformation);
            await _context.SaveChangesAsync();

            foreach (var tagModel in model.Tags)
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == tagModel);
                if (tag != null && merchantInformation.MerchantInformationTags.Any(m => m.Tag == tag))
                {
                    var merchantInformationTag = new MerchantInformationTag
                    {
                        MerchantInformation = merchantInformation,
                        Tag = tag
                    };
                    _context.MerchantInformationTags.Add(merchantInformationTag);

                    merchantInformation.MerchantInformationTags.Add(merchantInformationTag);
                    tag.MerchantInformationTags.Add(merchantInformationTag);
                }
            }

            if (model.Logo != null || model.Banner != null)
            {
                if (model.Logo != null)
                {
                    var logoContainerName = _optionsAccessor.Value.MerchantLogosContainerNameOption;
                    try
                    {
                        merchantInformation.Logo = await _blobStorageService.UploadAsync(logoContainerName,
                            $"{merchantInformation.CompanyName}-{merchantInformation.Id}-logo", model.Logo,
                            "image/png");
                    }
                    catch (FormatException ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }

                if (model.Banner != null)
                {
                    try
                    {
                        var bannerContainerName = _optionsAccessor.Value.MerchantBannersContainerNameOption;
                        merchantInformation.Banner = await _blobStorageService.UploadAsync(bannerContainerName,
                            $"{merchantInformation.CompanyName}-{merchantInformation.Id}-banner", model.Banner,
                            "image/png");
                    }
                    catch (FormatException ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }

                _context.MerchantInformations.Update(merchantInformation);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [Authorize(Roles = RefundeoConstants.RoleMerchant)]
        [HttpDelete]
        public async Task<IActionResult> DeleteMerchant()
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return _utilityService.GenerateBadRequestObjectResult("Merchant does not exist");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(result.Errors);
            }

            return new NoContentResult();
        }
    }
}

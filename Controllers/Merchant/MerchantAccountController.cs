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
        public async Task<IActionResult> GetAllMerchants()
        {
            var user = await _utilityService.GetCallingUserAsync(Request);

            if (user == null)
            {
                return BadRequest();
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains(RefundeoConstants.RoleAdmin))
            {
                var merchantInformations = await _context.MerchantInformations
                    .Include(i => i.Merchants)
                    .Include(i => i.Address)
                    .Include(i => i.Location)
                    .Include(i => i.OpeningHours)
                    .Include(i => i.FeePoints)
                    .Include(i => i.MerchantInformationTags)
                    .ThenInclude(i => i.Tag)
                    .ToListAsync();

                var dtos = new List<MerchantInformationDto>();

                foreach (var merchantInformation in merchantInformations)
                {
                    dtos.Add(await _utilityService.ConvertMerchantInformationToDtoAsync(merchantInformation));
                }

                return Ok(dtos);
            }

            if (roles.Contains(RefundeoConstants.RoleMerchant))
            {
                return Ok(await _context.MerchantInformations
                    .Include(i => i.Merchants)
                    .Include(i => i.Address)
                    .Include(i => i.Location)
                    .Include(i => i.OpeningHours)
                    .Include(i => i.FeePoints)
                    .Include(i => i.City)
                    .ThenInclude(c => c.Location)
                    .Include(i => i.MerchantInformationTags)
                    .ThenInclude(i => i.Tag)
                    .Select(i => _utilityService.ConvertMerchantInformationToSimpleDto(i))
                    .ToListAsync());
            }

            return Ok(await _context.MerchantInformations
                .Include(i => i.Merchants)
                .Include(i => i.Address)
                .Include(i => i.Location)
                .Include(i => i.OpeningHours)
                .Include(i => i.FeePoints)
                .Include(i => i.MerchantInformationTags)
                .ThenInclude(i => i.Tag)
                .Select(i => _utilityService.ConvertMerchantInformationToRestrictedDto(i))
                .ToListAsync());
        }

        [Authorize(Roles = RefundeoConstants.RoleMerchant + "," + RefundeoConstants.RoleAdmin)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);

            var merchantInformation = await _context.MerchantInformations
                .Include(i => i.Merchants)
                .Include(i => i.Address)
                .Include(i => i.Location)
                .Include(i => i.OpeningHours)
                .Include(i => i.FeePoints)
                .Include(i => i.City)
                .Include(i => i.MerchantInformationTags)
                .ThenInclude(i => i.Tag)
                .Where(i => i.Merchants.Any(x => x.Id == id))
                .FirstOrDefaultAsync();

            if (merchantInformation == null)
            {
                return BadRequest();
            }

            return await _userManager.IsInRoleAsync(user, RefundeoConstants.RoleAdmin)
                ? Ok(await _utilityService.ConvertMerchantInformationToDtoAsync(merchantInformation))
                : Ok(_utilityService.ConvertMerchantInformationToSimpleDto(merchantInformation));
        }

        [Authorize(Roles = RefundeoConstants.RoleAdmin)]
        [HttpPost]
        public async Task<IActionResult> RegisterMerchant([FromBody] MerchantRegisterDto model)
        {
            if (!ModelState.IsValid || model.Username == null || model.Password == null || model.City == null)
            {
                return BadRequest();
            }

            var city = await _context.Cities.FirstOrDefaultAsync(c => c.GooglePlaceId == model.City.GooglePlaceId);

            if (city == null)
            {
                return NotFound($"Could not find city with id = {model.City.GooglePlaceId}");
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
                PostalCode = model.AddressPostalCode
            };

            await _context.Addresses.AddAsync(address);

            var location = new Location
            {
                Latitude = model.Latitude,
                Longitude = model.Longitude
            };

            await _context.Locations.AddAsync(location);

            var merchantInformation = new MerchantInformation
            {
                CompanyName = model.CompanyName,
                CVRNumber = model.CvrNumber,
                Location = location,
                Address = address,
                VATRate = model.VatRate,
                Rating = model.Rating,
                PriceLevel = model.PriceLevel,
                Description = model.Description,
                ContactEmail = model.ContactEmail,
                ContactPhone = model.ContactPhone,
                AdminEmail = model.AdminEmail,
                VATNumber = model.VatNumber,
                Currency = model.Currency,
                DateCreated = DateTime.Now,
                City = city
            };

            await _context.MerchantInformations.AddAsync(merchantInformation);

            await _context.SaveChangesAsync();

            if (_context.Entry(merchantInformation).Collection(x => x.FeePoints).IsLoaded == false)
            {
                await _context.Entry(merchantInformation).Collection(x => x.FeePoints).LoadAsync();
            }

            var vatPercentage = 100 - 100 / (1 + model.VatRate / 100);

            foreach (var feePointModel in model.FeePoints)
            {
                var adminPercentage = vatPercentage * (feePointModel.AdminFee / 100);
                var merchantPercantage = vatPercentage * (feePointModel.MerchantFee / 100);

                var feePoint = new FeePoint
                {
                    AdminFee = feePointModel.AdminFee,
                    End = feePointModel.End,
                    MerchantFee = feePointModel.MerchantFee,
                    Start = feePointModel.Start,
                    RefundPercentage = vatPercentage - adminPercentage - merchantPercantage
                };

                await _context.FeePoints.AddAsync(feePoint);

                merchantInformation.FeePoints.Add(feePoint);
            }

            if (_context.Entry(merchantInformation).Collection(x => x.Merchants).IsLoaded == false)
            {
                await _context.Entry(merchantInformation).Collection(x => x.Merchants).LoadAsync();
            }

            merchantInformation.Merchants.Add(user);

            foreach (var tagModel in model.Tags)
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Key == tagModel);
                if (tag != null)
                {
                    var merchantInformationTag = new MerchantInformationTag
                    {
                        MerchantInformation = merchantInformation,
                        Tag = tag
                    };
                    await _context.MerchantInformationTags.AddAsync(merchantInformationTag);

                    merchantInformation.MerchantInformationTags.Add(merchantInformationTag);
                    tag.MerchantInformationTags.Add(merchantInformationTag);
                }
            }

            if (_context.Entry(merchantInformation).Collection(x => x.OpeningHours).IsLoaded == false)
            {
                await _context.Entry(merchantInformation).Collection(x => x.OpeningHours).LoadAsync();
            }

            foreach (var openingHoursModel in model.OpeningHours)
            {
                var openingHours = new OpeningHours
                {
                    Close = openingHoursModel.Close,
                    Day = openingHoursModel.Day,
                    Open = openingHoursModel.Open
                };
                await _context.OpeningHours.AddAsync(openingHours);

                merchantInformation.OpeningHours.Add(openingHours);
            }

            await _context.SaveChangesAsync();

            if (model.Logo == null && model.Banner == null)
                return await _authenticationService.GenerateTokenResultAsync(user);

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

            return await _authenticationService.GenerateTokenResultAsync(user);
        }

        [Authorize(Roles = RefundeoConstants.RoleMerchant)]
        [HttpPut]
        public async Task<IActionResult> ChangeMerchant([FromBody] ChangeMerchantRestrictedDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Keys);
            }

            if (model.FeePoints.Any(f => f.MerchantFee > 50))
            {
                return BadRequest("No feepoints can be above 50 %");
            }

            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return _utilityService.GenerateBadRequestObjectResult("Merchant does not exist");
            }

            var merchantInformation = await _context.MerchantInformations
                .Include(i => i.Merchants)
                .Include(m => m.Address)
                .Include(m => m.Location)
                .Include(m => m.OpeningHours)
                .Include(m => m.FeePoints)
                .Include(m => m.MerchantInformationTags)
                .ThenInclude(m => m.Tag)
                .FirstOrDefaultAsync(i => i.Merchants.Any(x => x.Id == user.Id));

            var city = await _context.Cities
                .FirstOrDefaultAsync(c => c.GooglePlaceId == model.City.GooglePlaceId);

            if (merchantInformation == null || city == null)
            {
                return NotFound();
            }

            if (merchantInformation.City == null || merchantInformation.City.Id != city.Id)
            {
                merchantInformation.City = city;
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

            var vatPercentage = 100 - 100 / (1 + merchantInformation.VATRate / 100);

            foreach (var feePointModel in model.FeePoints)
            {
                var feePoint =
                    merchantInformation
                        .FeePoints
                        .FirstOrDefault(f => Math.Abs(f.Start - feePointModel.Start) < 0.1);

                if (feePoint == null)
                {
                    return BadRequest($"No feepoint with start: {feePointModel.Start} was found.");
                }

                var adminPercentage = vatPercentage * (feePoint.AdminFee / 100);
                var merchantPercantage = vatPercentage * (feePointModel.MerchantFee / 100);

                feePoint.MerchantFee = feePointModel.MerchantFee;
                feePoint.RefundPercentage = vatPercentage - adminPercentage - merchantPercantage;

                _context.FeePoints.Update(feePoint);
            }

            merchantInformation.CompanyName = model.CompanyName;
            merchantInformation.Address.StreetName = model.AddressStreetName;
            merchantInformation.Address.Country = model.AddressCountry;
            merchantInformation.Address.City = model.AddressCity;
            merchantInformation.Address.StreetNumber = model.AddressStreetNumber;
            merchantInformation.Address.PostalCode = model.AddressPostalCode;
            merchantInformation.Location.Latitude = model.Latitude;
            merchantInformation.Location.Longitude = model.Longitude;
            merchantInformation.Description = model.Description;
            merchantInformation.ContactEmail = model.ContactEmail;
            merchantInformation.ContactPhone = model.ContactPhone;
            merchantInformation.AdminEmail = model.AdminEmail;
            merchantInformation.Currency = model.Currency;

            _context.MerchantInformations.Update(merchantInformation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = RefundeoConstants.RoleAdmin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> ChangeMerchant(string id, [FromBody] ChangeMerchantDto model)
        {
            if (!ModelState.IsValid || model.City == null)
            {
                return BadRequest(ModelState.Keys);
            }

            var merchantInformation = await _context.MerchantInformations
                .Include(i => i.Merchants)
                .Include(m => m.Address)
                .Include(m => m.Location)
                .Include(m => m.OpeningHours)
                .Include(m => m.FeePoints)
                .Include(m => m.City)
                .Include(m => m.MerchantInformationTags)
                .ThenInclude(m => m.Tag)
                .FirstOrDefaultAsync(i => i.Merchants.Any(x => x.Id == id));

            var city = await _context.Cities
                .FirstOrDefaultAsync(c => c.GooglePlaceId == model.City.GooglePlaceId);

            if (merchantInformation == null || city == null)
            {
                return NotFound();
            }

            if (merchantInformation.City == null || merchantInformation.City.Id != city.Id)
            {
                merchantInformation.City = city;
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

            var vatPercentage = 100 - 100 / (1 + model.VatRate / 100);

            foreach (var feePointModel in model.FeePoints)
            {
                var existingFeePoint =
                    merchantInformation
                        .FeePoints
                        .FirstOrDefault(o => Math.Abs(o.Start - feePointModel.Start) < 0.1);

                var adminPercentage = vatPercentage * (feePointModel.AdminFee / 100);
                var merchantPercentage = vatPercentage * (feePointModel.MerchantFee / 100);

                if (existingFeePoint == null)
                {
                    var feePoint = new FeePoint
                    {
                        Start = feePointModel.Start,
                        End = feePointModel.End,
                        AdminFee = feePointModel.AdminFee,
                        MerchantFee = feePointModel.MerchantFee,
                        RefundPercentage = vatPercentage - adminPercentage - merchantPercentage
                    };

                    _context.FeePoints.Add(feePoint);

                    merchantInformation.FeePoints.Add(feePoint);
                }
                else
                {
                    existingFeePoint.Start = feePointModel.Start;
                    existingFeePoint.End = feePointModel.End;
                    existingFeePoint.AdminFee = feePointModel.AdminFee;
                    existingFeePoint.MerchantFee = feePointModel.MerchantFee;
                    existingFeePoint.RefundPercentage = vatPercentage - adminPercentage - merchantPercentage;

                    _context.FeePoints.Update(existingFeePoint);
                }
            }

            var feePointsToDelete = merchantInformation.FeePoints
                .Where(f => model.FeePoints.All(mf => Math.Abs(mf.Start - f.Start) > 0.1)).ToList();

            if (Math.Abs(model.VatRate - merchantInformation.VATRate) > 0.1)
            {
                foreach (var feePoint in merchantInformation.FeePoints)
                {
                    var adminPercentage = vatPercentage * (feePoint.AdminFee / 100);
                    var merchantPercantage = vatPercentage * (feePoint.MerchantFee / 100);

                    feePoint.RefundPercentage = vatPercentage - adminPercentage - merchantPercantage;

                    _context.FeePoints.Update(feePoint);
                }
            }

            _context.RemoveRange(feePointsToDelete);

            merchantInformation.CompanyName = model.CompanyName;
            merchantInformation.CVRNumber = model.CvrNumber;
            merchantInformation.PriceLevel = model.PriceLevel;
            merchantInformation.Address.StreetName = model.AddressStreetName;
            merchantInformation.Address.Country = model.AddressCountry;
            merchantInformation.Address.City = model.AddressCity;
            merchantInformation.Address.StreetNumber = model.AddressStreetNumber;
            merchantInformation.Address.PostalCode = model.AddressPostalCode;
            merchantInformation.Location.Latitude = model.Latitude;
            merchantInformation.Location.Longitude = model.Longitude;
            merchantInformation.Description = model.Description;
            merchantInformation.VATRate = model.VatRate;
            merchantInformation.Rating = model.Rating;
            merchantInformation.VATNumber = model.VatNumber;
            merchantInformation.ContactEmail = model.ContactEmail;
            merchantInformation.ContactPhone = model.ContactPhone;
            merchantInformation.AdminEmail = model.AdminEmail;
            merchantInformation.Currency = model.Currency;

            _context.MerchantInformations.Update(merchantInformation);
            await _context.SaveChangesAsync();

            foreach (var tagModel in model.Tags)
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Key == tagModel);

                if (tag == null) continue;

                var merchantInformationTag =
                    await _context.MerchantInformationTags.FirstOrDefaultAsync(x =>
                        x.TagId == tag.Id && x.MerhantInformationId == merchantInformation.Id);

                if (merchantInformationTag != null) continue;

                merchantInformationTag = new MerchantInformationTag
                {
                    MerchantInformation = merchantInformation,
                    Tag = tag
                };
                _context.MerchantInformationTags.Add(merchantInformationTag);
            }

            foreach (var merchantInformationTag in merchantInformation.MerchantInformationTags)
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == merchantInformationTag.TagId);

                if (!model.Tags.Contains(tag.Key))
                {
                    _context.Remove(merchantInformationTag);
                }
            }

            if (model.Logo == null && model.Banner == null)
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }

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

            var result = await _authenticationService.DeleteUserAsync(user);
            if (!result.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(result.Errors);
            }

            return new NoContentResult();
        }
    }
}

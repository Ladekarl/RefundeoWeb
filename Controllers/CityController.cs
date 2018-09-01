using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Refundeo.Core.Data;
using Refundeo.Core.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using Microsoft.Extensions.Options;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Controllers
{
    [Route("/api/city")]
    public class CityController : Controller
    {
        private readonly RefundeoDbContext _context;
        private readonly IOptions<StorageAccountOptions> _optionsAccessor;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IUtilityService _utilityService;

        public CityController(
            RefundeoDbContext context,
            IOptions<StorageAccountOptions> optionsAccessor,
            IBlobStorageService blobStorageService, IUtilityService utilityService)
        {
            _context = context;
            _optionsAccessor = optionsAccessor;
            _blobStorageService = blobStorageService;
            _utilityService = utilityService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IList<CityDto>> GetAllCities()
        {
            return await _context.Cities
                .Include(c => c.Location)
                .Select(c => new CityDto
                {
                    Name = c.Name,
                    GooglePlaceId = c.GooglePlaceId,
                    Latitude = c.Location.Latitude,
                    Longitude = c.Location.Longitude,
                    Image = c.Image
                }).ToListAsync();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCityById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var city = await _context.Cities
                .Include(c => c.Location)
                .Include(c => c.MerchantInformations).ThenInclude(m => m.Merchants)
                .Include(c => c.MerchantInformations).ThenInclude(m => m.Address)
                .Include(c => c.MerchantInformations).ThenInclude(m => m.Location)
                .Include(c => c.MerchantInformations).ThenInclude(m => m.OpeningHours)
                .Include(c => c.MerchantInformations).ThenInclude(m => m.FeePoints)
                .Include(c => c.MerchantInformations).ThenInclude(m => m.MerchantInformationTags)
                .ThenInclude(mt => mt.Tag)
                .FirstOrDefaultAsync(c => c.GooglePlaceId == id);

            if (city == null)
            {
                return NotFound();
            }

            var dto = new CityDto
            {
                GooglePlaceId = city.GooglePlaceId,
                Name = city.Name,
                Image = city.Image,
                Latitude = city.Location.Latitude,
                Longitude = city.Location.Longitude,
                Merchants =
                    city.MerchantInformations
                        .Select(m => _utilityService.ConvertMerchantInformationToRestrictedDto(m))
                        .ToList()
            };

            return Ok(dto);
        }

        [Authorize(Roles = RefundeoConstants.RoleAdmin)]
        [HttpPost]
        public async Task<IActionResult> CreateCity([FromBody] CityDto model)
        {
            if (!ModelState.IsValid ||
                string.IsNullOrEmpty(model.Name) ||
                string.IsNullOrEmpty(model.GooglePlaceId) ||
                string.IsNullOrEmpty(model.Image))
            {
                return BadRequest();
            }

            var city = new City
            {
                Name = model.Name,
                GooglePlaceId = model.GooglePlaceId
            };

            var location = new Location
            {
                Latitude = model.Latitude,
                Longitude = model.Longitude
            };

            var cityImageContainerName = _optionsAccessor.Value.CityImagesContainerNameOption;
            city.Image = await _blobStorageService.UploadAsync(cityImageContainerName,
                $"{model.Name}-{model.GooglePlaceId}", model.Image,
                "image/png");

            await _context.Locations.AddAsync(location);

            await _context.SaveChangesAsync();

            city.Location = location;

            await _context.Cities.AddAsync(city);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = RefundeoConstants.RoleAdmin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCity(string id, [FromBody] CityDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var city = await _context.Cities
                .Include(c => c.Location)
                .FirstOrDefaultAsync(c => c.GooglePlaceId == id);

            if (city == null)
            {
                return NotFound();
            }

            if (city.Location == null)
            {
                var location = new Location
                {
                    Longitude = model.Longitude,
                    Latitude = model.Latitude
                };
                await _context.Locations.AddAsync(location);
                await _context.SaveChangesAsync();
                city.Location = location;
            }
            else
            {
                city.Location.Latitude = model.Latitude;
                city.Location.Longitude = model.Longitude;
                _context.Locations.Update(city.Location);
            }

            city.Name = model.Name;

            if (!string.IsNullOrEmpty(model.Image))
            {
                try
                {
                    var cityImageContainerName = _optionsAccessor.Value.CityImagesContainerNameOption;
                    city.Image = await _blobStorageService.UploadAsync(cityImageContainerName,
                        $"{model.Name}-{model.GooglePlaceId}", model.Image,
                        "image/png");
                }
                catch
                {
                    // IGNORED
                }
            }

            _context.Cities.Update(city);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = RefundeoConstants.RoleAdmin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCity(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var city = await _context.Cities.FirstOrDefaultAsync(c => c.GooglePlaceId == id);

            if (city == null)
            {
                return NotFound();
            }

            await _blobStorageService.DeleteAsync(new Uri(city.Image));
            _context.Remove(city);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

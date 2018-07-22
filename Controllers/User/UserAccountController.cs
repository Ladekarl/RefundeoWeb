using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Refundeo.Core.Helpers;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Models.Account;
using Refundeo.Core.Services.Interfaces;
using System.Linq;

namespace Refundeo.Controllers.User
{
    [Route("/api/user/account")]
    public class UserAccountController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly UserManager<RefundeoUser> _userManager;
        private readonly IUtilityService _utilityService;
        private readonly RefundeoDbContext _context;

        public UserAccountController(RefundeoDbContext context, UserManager<RefundeoUser> userManager,
            IAuthenticationService authenticationService, IUtilityService utilityService)
        {
            _authenticationService = authenticationService;
            _userManager = userManager;
            _utilityService = utilityService;
            _context = context;
        }

        [Authorize(Roles = RefundeoConstants.RoleAdmin)]
        [HttpGet]
        public async Task<IList<CustomerInformationDto>> GetAllCustomers()
        {
            var userModels = new List<CustomerInformationDto>();
            foreach (var u in await _context.CustomerInformations
                .Include(i => i.Customer)
                .Include(i => i.Address)
                .AsNoTracking()
                .ToListAsync())
            {
                userModels.Add(await _utilityService.ConvertCustomerInformationToDtoAsync(u));
            }

            return userModels;
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(string id)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);

            if (user == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, RefundeoConstants.RoleAdmin);

            var customer = await _context.CustomerInformations
                .Include(c => c.Address)
                .Include(c => c.Customer)
                .Where(c => c.Customer.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return NotFound();
            }


            if (user.Id != id && !isAdmin)
            {
                return Ok(_utilityService.ConvertCustomerInformationToSimpleDto(customer));
            }

            return Ok(await _utilityService.ConvertCustomerInformationToDtoAsync(customer));
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = new RefundeoUser {UserName = model.Username};

            var customerInformation = new CustomerInformation
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Country = model.Country,
                Email = model.Email,
                Phone = model.Phone,
                AcceptedPrivacyPolicy = model.AcceptedPrivacyPolicy,
                AcceptedTermsOfService = model.AcceptedTermsOfService,
                PrivacyPolicy = model.PrivacyPolicy,
                TermsOfService = model.TermsOfService,
                AccountNumber = model.AccountNumber,
                Swift = model.Swift,
                Passport = model.Passport,
                Language = model.Language,
                Address = new Address
                {
                    City = model.AddressCity,
                    Country = model.AddressCountry,
                    PostalCode = model.AddressPostalCode,
                    StreetName = model.AddressStreetName,
                    StreetNumber = model.AddressStreetNumber
                }
            };

            var shouldCreateRefreshToken = model.Scopes != null && model.Scopes.Contains("offline_access");

            return await _authenticationService.RegisterUserAsync(user, model.Password, customerInformation,
                shouldCreateRefreshToken);
        }

        [Authorize(Roles = RefundeoConstants.RoleUser)]
        [HttpPut]
        public async Task<IActionResult> ChangeUser([FromBody] ChangeUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return NotFound();
            }

            var customerInformation = await _context.CustomerInformations
                .Include(i => i.Customer)
                .Include(i => i.Address)
                .Where(i => i.Customer == user)
                .FirstOrDefaultAsync();

            if (customerInformation == null)
            {
                return NotFound();
            }

            if (customerInformation.Address == null)
            {
                customerInformation.Address = new Address();
                await _context.Addresses.AddAsync(customerInformation.Address);
            }

            if (user.UserName != model.Username)
            {
                user.UserName = model.Username;

                var updateUserResult = await _userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                {
                    return _utilityService.GenerateBadRequestObjectResult(updateUserResult.Errors);
                }
            }

            customerInformation.FirstName = model.FirstName;
            customerInformation.LastName = model.LastName;
            customerInformation.Country = model.Country;
            customerInformation.Email = model.Email;
            customerInformation.Phone = model.Phone;
            customerInformation.AcceptedPrivacyPolicy = model.AcceptedPrivacyPolicy;
            customerInformation.AcceptedTermsOfService = model.AcceptedTermsOfService;
            customerInformation.PrivacyPolicy = model.PrivacyPolicy;
            customerInformation.TermsOfService = model.TermsOfService;
            customerInformation.Swift = model.Swift;
            customerInformation.AccountNumber = model.AccountNumber;
            customerInformation.Passport = model.Passport;
            customerInformation.Address.StreetName = model.AddressStreetName;
            customerInformation.Address.StreetNumber = model.AddressStreetNumber;
            customerInformation.Address.PostalCode = model.AddressPostalCode;
            customerInformation.Address.Country = model.AddressCountry;
            customerInformation.Address.City = model.AddressCity;
            customerInformation.Language = model.Language;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = RefundeoConstants.RoleUser)]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser()
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return _utilityService.GenerateBadRequestObjectResult("User does not exist");
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

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
            foreach (var u in await _context.CustomerInformations.Include(i => i.Customer).ToListAsync())
            {
                userModels.Add(_utilityService.ConvertCustomerInformationToDto(u));
            }

            return userModels;
        }

        [Authorize(Roles = RefundeoConstants.RoleAdmin + "," + RefundeoConstants.RoleUser)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(string id)
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            var isAdmin = await _userManager.IsInRoleAsync(user, RefundeoConstants.RoleAdmin);
            if (user.Id != id && !isAdmin)
            {
                return StatusCode(403);
            }

            var customer = await _context.CustomerInformations
                .Where(c => c.Customer.Id == user.Id)
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return NotFound();
            }

            return Ok(_utilityService.ConvertCustomerInformationToDto(customer));
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
                AcceptedPrivacyPolicy = model.AcceptedPrivacyPolicy,
                AcceptedTermsOfService = model.AcceptedTermsOfService,
                PrivacyPolicy = model.PrivacyPolicy,
                TermsOfService = model.TermsOfService
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
                .FirstOrDefaultAsync(i => i.Customer == user);

            if (customerInformation == null)
            {
                return NotFound();
            }

            user.UserName = model.Username;

            var updateUserResult = await _userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(updateUserResult.Errors);
            }

            customerInformation.FirstName = model.FirstName;
            customerInformation.LastName = model.LastName;
            customerInformation.Country = model.Country;
            customerInformation.BankAccountNumber = model.BankAccountNumber;
            customerInformation.BankRegNumber = model.BankRegNumber;
            customerInformation.AcceptedPrivacyPolicy = model.AcceptedPrivacyPolicy;
            customerInformation.AcceptedTermsOfService = model.AcceptedTermsOfService;
            customerInformation.PrivacyPolicy = model.PrivacyPolicy;
            customerInformation.TermsOfService = model.TermsOfService;

            await _context.SaveChangesAsync();

            return new NoContentResult();
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

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(result.Errors);
            }

            return new NoContentResult();
        }
    }
}

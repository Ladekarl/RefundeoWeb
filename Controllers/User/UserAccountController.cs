using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Refundeo.Data;
using Refundeo.Data.Models;
using Refundeo.Models.Account;

namespace Refundeo.Controllers.User
{
    [Route("/api/user/account")]
    public class UserAccountController : AuthenticationController
    {
        public UserAccountController(RefundeoDbContext context, IConfiguration config, UserManager<RefundeoUser> userManager, SignInManager<RefundeoUser> signManager) : base(context, config, userManager, signManager)
        {
        }

        [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
        [HttpGet]
        public async Task<IList<CustomerInformationDTO>> GetAllCustomers()
        {
            var userModels = new List<CustomerInformationDTO>();
            foreach (var u in await context.CustomerInformations.Include(i => i.Customer).ToListAsync())
            {
                userModels.Add(ConvertCustomerInformationToDTO(u));
            }
            return userModels;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = new RefundeoUser { UserName = model.Username };
            var createUserResult = await userManager.CreateAsync(user, model.Password);

            if (!createUserResult.Succeeded)
            {
                return GenerateBadRequestObjectResult(createUserResult.Errors);
            }

            var addToRoleResult = await userManager.AddToRoleAsync(user, RefundeoConstants.ROLE_USER);

            await context.SaveChangesAsync();

            if (!addToRoleResult.Succeeded)
            {
                return GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            var customerInformation = new CustomerInformation
            {
                FirstName = model.Firstname,
                LastName = model.Lastname,
                Country = model.Country
            };

            await context.CustomerInformations.AddAsync(customerInformation);
            await context.SaveChangesAsync();

            return await GenerateTokenResultAsync(user);
        }

        [Authorize(Roles = RefundeoConstants.ROLE_USER)]
        [HttpPut]
        public async Task<IActionResult> ChangeUser([FromBody] ChangeUserDTO model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            var user = await GetCallingUserAsync();
            if (user == null)
            {
                return NotFound();
            }

            var customerInformation = await context.CustomerInformations
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.Customer == user);

            if (customerInformation == null)
            {
                return NotFound();
            }

            user.UserName = model.Username;

            var updateUserResult = await userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                return GenerateBadRequestObjectResult(updateUserResult.Errors);
            }

            customerInformation.FirstName = model.Firstname;
            customerInformation.LastName = model.Lastname;
            customerInformation.Country = model.Country;

            await context.SaveChangesAsync();

            return new NoContentResult();
        }

        [Authorize(Roles = RefundeoConstants.ROLE_USER)]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser()
        {
            var user = await GetCallingUserAsync();
            if (user == null)
            {
                return GenerateBadRequestObjectResult($"User does not exist");
            }

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return GenerateBadRequestObjectResult(result.Errors);
            }

            return new NoContentResult();
        }
    }
}
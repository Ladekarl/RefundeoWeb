using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.Account;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Controllers.Admin
{
    [Authorize(Roles = RefundeoConstants.ROLE_ADMIN)]
    [Route("/api/admin/account")]
    public class AdminAccountController : Controller
    {
        private RefundeoDbContext _context;
        private UserManager<RefundeoUser> _userManager;
        private IUtilityService _utilityService;
        private IAuthenticationService _authenticationService;
        public AdminAccountController(RefundeoDbContext context, UserManager<RefundeoUser> userManager, IUtilityService utilityService, IAuthenticationService authenticationService)
        {
            _context = context;
            _userManager = userManager;
            _utilityService = utilityService;
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public async Task<IList<UserDTO>> GetAllAdmins()
        {
            var userModels = new List<UserDTO>();
            foreach (var u in await _userManager.GetUsersInRoleAsync(RefundeoConstants.ROLE_ADMIN))
            {
                userModels.Add(await _utilityService.ConvertRefundeoUserToUserDTOAsync(u));
            }
            return userModels;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = new RefundeoUser { UserName = model.Username };
            var createUserResult = await _userManager.CreateAsync(user, model.Password);

            if (!createUserResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(createUserResult.Errors);
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, RefundeoConstants.ROLE_ADMIN);

            if (!addToRoleResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(addToRoleResult.Errors);
            }

            return await _authenticationService.GenerateTokenResultAsync(user, null);
        }

        [HttpPut]
        public async Task<IActionResult> ChangeAdmin([FromBody] ChangeAdminDTO model)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(model.Username))
            {
                return BadRequest();
            }

            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return _utilityService.GenerateBadRequestObjectResult("Admin does not exist");
            }

            user.UserName = model.Username;

            var updateUserResult = await _userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                return _utilityService.GenerateBadRequestObjectResult(updateUserResult.Errors);
            }

            return new NoContentResult();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAdmin()
        {
            var user = await _utilityService.GetCallingUserAsync(Request);
            if (user == null)
            {
                return _utilityService.GenerateBadRequestObjectResult($"Admin does not exist");
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

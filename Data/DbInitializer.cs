using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Refundeo.Controllers;
using Refundeo.Data.Models;

namespace Refundeo.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(UserManager<RefundeoUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await InitializeRolesAsync(roleManager);
            await InitializeUsersAsync(userManager);
        }

        private static async Task InitializeRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(RefundeoConstants.ROLE_ADMIN))
            {
                await CreateRoleAsync(roleManager, RefundeoConstants.ROLE_ADMIN);
            }
            if (!await roleManager.RoleExistsAsync(RefundeoConstants.ROLE_MERCHANT))
            {
                await CreateRoleAsync(roleManager, RefundeoConstants.ROLE_MERCHANT);
            }
            if (!await roleManager.RoleExistsAsync(RefundeoConstants.ROLE_USER))
            {
                await CreateRoleAsync(roleManager, RefundeoConstants.ROLE_USER);
            }
        }

        private static async Task InitializeUsersAsync(UserManager<RefundeoUser> userManager)
        {

            if (!userManager.Users.Any(u => u.UserName == "Admin"))
            {
                await CreateUserAsync(userManager, "Admin", "Admin1234!", RefundeoConstants.ROLE_ADMIN);
            }
            if (!userManager.Users.Any(u => u.UserName == "Merchant"))
            {
                await CreateUserAsync(userManager, "Merchant", "Merchant1234!", RefundeoConstants.ROLE_MERCHANT);
            }
            if (!userManager.Users.Any(u => u.UserName == "User"))
            {
                await CreateUserAsync(userManager, "User", "User1234!", RefundeoConstants.ROLE_USER);
            }
        }

        private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            var role = new IdentityRole();
            role.Name = roleName;
            await roleManager.CreateAsync(role);
        }

        private static async Task CreateUserAsync(UserManager<RefundeoUser> userManager, string username, string password, string role)
        {
            var user = new RefundeoUser { UserName = username };
            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
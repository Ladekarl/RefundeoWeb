using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await CreateRoleAsync(roleManager, "Admin");
            }
            if (!await roleManager.RoleExistsAsync("Merchant"))
            {
                await CreateRoleAsync(roleManager, "Merchant");
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await CreateRoleAsync(roleManager, "User");
            }
        }

        private static async Task InitializeUsersAsync(UserManager<RefundeoUser> userManager)
        {

            if (!userManager.Users.Any(u => u.UserName == "Admin"))
            {
                await CreateUserAsync(userManager, "Admin", "Admin1234!", "Admin");
            }
            if (!userManager.Users.Any(u => u.UserName == "Merchant"))
            {
                await CreateUserAsync(userManager, "Merchant", "Merchant1234!", "Merchant");
            }
            if (!userManager.Users.Any(u => u.UserName == "User"))
            {
                await CreateUserAsync(userManager, "User", "User1234!", "User");
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
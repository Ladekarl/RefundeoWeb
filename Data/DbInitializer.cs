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
        public static async Task InitializeAsync(UserManager<RefundeoUser> userManager, RoleManager<IdentityRole> roleManager, RefundeoDbContext context)
        {
            await InitializeRolesAsync(roleManager);
            await InitializeUsersAsync(userManager, context);
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

        private static async Task InitializeUsersAsync(UserManager<RefundeoUser> userManager, RefundeoDbContext context)
        {

            if (!userManager.Users.Any(u => u.UserName == "Admin"))
            {
                await CreateAccountAsync(userManager, "Admin", "Admin1234!", RefundeoConstants.ROLE_ADMIN);
            }
            if (!userManager.Users.Any(u => u.UserName == "Merchant"))
            {
                await CreateMerchantAsync(userManager, context, "Merchant", "Merchant1234!", "MerchantCompany", "12345678", 25);
            }
            if (!userManager.Users.Any(u => u.UserName == "User"))
            {
                await CreateCustomerAsync(userManager, context, "User", "User1234!", "Bob", "Dylan", "DK", "123456781234", "1234");
            }
        }

        private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            var role = new IdentityRole();
            role.Name = roleName;
            await roleManager.CreateAsync(role);
        }

        private static async Task CreateCustomerAsync(UserManager<RefundeoUser> userManager, RefundeoDbContext context, string username, string password, string firstName, string lastName, string country, string bankAccountNumber, string bankRegNumber)
        {
            var user = await CreateAccountAsync(userManager, username, password, RefundeoConstants.ROLE_USER);
            if (user != null)
            {
                await context.CustomerInformations.AddAsync(new CustomerInformation
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Country = country,
                    BankAccountNumber = bankAccountNumber,
                    BankRegNumber = bankRegNumber,
                    Customer = user
                });
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateMerchantAsync(UserManager<RefundeoUser> userManager, RefundeoDbContext context, string username, string password, string companyName, string cvrNumber, int refundPercentage)
        {
            var user = await CreateAccountAsync(userManager, username, password, RefundeoConstants.ROLE_MERCHANT);
            if (user != null)
            {
                await context.MerchantInformations.AddAsync(new MerchantInformation
                {
                    CompanyName = companyName,
                    CVRNumber = cvrNumber,
                    RefundPercentage = refundPercentage,
                    Merchant = user
                });
                await context.SaveChangesAsync();
            }
        }

        private static async Task<RefundeoUser> CreateAccountAsync(UserManager<RefundeoUser> userManager, string username, string password, string role)
        {
            var user = new RefundeoUser { UserName = username };
            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                result = await userManager.AddToRoleAsync(user, role);
            }

            if (result.Succeeded)
            {
                return user;
            }

            return null;
        }
    }
}
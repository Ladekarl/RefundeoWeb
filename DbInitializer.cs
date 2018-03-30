using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Refundeo.Models;

namespace Refundeo
{
    public static class DbInitializer
    {
        public static void Initialize(UserManager<RefundeoUser> userManager) 
        {
            if(!userManager.Users.Any(u => u.UserName == "Test")) 
            {
                var user = new RefundeoUser {UserName = "Test"};
                var result = userManager.CreateAsync(user, "Test123!");
            }
            return;
        }        
    }
}
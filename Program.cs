using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Initializers;
using Refundeo.Core.Data.Models;

namespace Refundeo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var env = services.GetRequiredService<IHostingEnvironment>();
                    if (env.IsDevelopment())
                    {
                        var context = services.GetRequiredService<RefundeoDbContext>();
                        var userManager = services.GetRequiredService<UserManager<RefundeoUser>>();
                        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                        DbInitializer.InitializeAsync(userManager, roleManager, context).Wait();
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();
    }
}

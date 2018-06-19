using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.QRCode;
using ZXing;
using ZXing.QrCode;

namespace Refundeo.Core.Data.Initializers
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(UserManager<RefundeoUser> userManager,
            RoleManager<IdentityRole> roleManager, RefundeoDbContext context)
        {
            await InitializeRolesAsync(roleManager);
            await InitializeUsersAsync(userManager, context);
            await InitializeRefundCasesAsync(context);
        }

        private static async Task InitializeRefundCasesAsync(RefundeoDbContext context)
        {
            foreach (var refundCase in DbInitializeData.RefundCasesToCreate)
            {
                for (var i = 0; i <= 10; i++)
                {
                    var existingCase = await context.RefundCases
                        .Include(r => r.MerchantInformation)
                        .ThenInclude(m => m.Merchant)
                        .FirstOrDefaultAsync(r =>
                            r.MerchantInformation.Merchant.UserName == refundCase.MerchantName &&
                            Math.Abs(r.Amount - refundCase.Amount) < 0.1);
                    if (existingCase == null)
                    {
                        await CreateRefundCaseAsync(context, refundCase);
                    }

                    refundCase.Amount++;
                }
            }
        }

        private static async Task InitializeRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in DbInitializeData.RolesToCreate)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await CreateRoleAsync(roleManager, role);
                }
            }
        }

        private static async Task InitializeUsersAsync(UserManager<RefundeoUser> userManager, RefundeoDbContext context)
        {
            foreach (var user in DbInitializeData.UsersTocreate)
            {
                if (!userManager.Users.Any(u => u.UserName == user.Username))
                {
                    await CreateCustomerAsync(userManager, context, user.Username, user.Password, user.FirstName,
                        user.LastName, user.Country, "123456781234");
                }
            }

            foreach (var admin in DbInitializeData.AdminsToCreate)
            {
                if (!userManager.Users.Any(u => u.UserName == admin.Username))
                {
                    await CreateAccountAsync(userManager, admin.Username, admin.Password, RefundeoConstants.RoleAdmin);
                }
            }

            foreach (var merchant in DbInitializeData.MerchantsToCreate)
            {
                if (!userManager.Users.Any(u => u.UserName == merchant.Username))
                {
                    var location = new Location
                    {
                        Latitude = merchant.Latitude,
                        Longitude = merchant.Longitude
                    };
                    var address = new Address
                    {
                        City = merchant.AddressCity,
                        Country = merchant.AddressCountry,
                        PostalCode = merchant.AddressPostalCode,
                        StreetName = merchant.AddressStreetName,
                        StreetNumber = merchant.AddressStreetNumber
                    };
                    await CreateMerchantAsync(userManager, context, merchant.Username, merchant.Password,
                        merchant.CompanyName, merchant.CvrNumber, merchant.RefundPercentage, merchant.OpeningHours,
                        merchant.Description, address, location);
                }
            }
        }

        private static async Task CreateRefundCaseAsync(RefundeoDbContext context,
            DbInitializeRefundCase dbInitializeRefundCase)
        {
            var merchantInformation = await context.MerchantInformations
                .Include(m => m.Merchant)
                .Where(i => i.Merchant.UserName == dbInitializeRefundCase.MerchantName)
                .FirstOrDefaultAsync();
            var customerInformation = await context.CustomerInformations
                .Include(m => m.Customer)
                .Where(i => i.Customer.UserName == dbInitializeRefundCase.CustomerName)
                .FirstOrDefaultAsync();

            if (merchantInformation == null)
            {
                return;
            }

            var refundCase = new RefundCase
            {
                Amount = dbInitializeRefundCase.Amount,
                RefundAmount = dbInitializeRefundCase.Amount,
                MerchantInformation = merchantInformation,
                IsRequested = dbInitializeRefundCase.IsRequested,
                DateCreated = DateTime.UtcNow,
                DateRequested = dbInitializeRefundCase.DateRequested
            };

            if (customerInformation != null)
            {
                refundCase.CustomerInformation = customerInformation;
            }

            await context.RefundCases.AddAsync(refundCase);

            await context.SaveChangesAsync();

            var qrCode = new QRCode
            {
                Image = GenerateQrCode(dbInitializeRefundCase.QrCodeHeight, dbInitializeRefundCase.QrCodeWidth,
                    dbInitializeRefundCase.QrCodeMargin, new QRCodeRefundCaseDto
                    {
                        RefundCaseId = refundCase.Id
                    })
            };
            await context.QRCodes.AddAsync(qrCode);
            refundCase.QRCode = qrCode;
            context.RefundCases.Update(refundCase);
            await context.SaveChangesAsync();
        }

        private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            var role = new IdentityRole {Name = roleName};
            await roleManager.CreateAsync(role);
        }

        private static async Task CreateCustomerAsync(UserManager<RefundeoUser> userManager, RefundeoDbContext context,
            string username, string password, string firstName, string lastName, string country,
            string swift)
        {
            var user = await CreateAccountAsync(userManager, username, password, RefundeoConstants.RoleUser);
            if (user != null)
            {
                await context.CustomerInformations.AddAsync(new CustomerInformation
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Country = country,
                    Swift = swift,
                    Customer = user
                });
                await context.SaveChangesAsync();
            }
        }

        private static byte[] GenerateQrCode(int height, int width, int margin, QRCodeRefundCaseDto refundCase)
        {
            var qrCodeWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width,
                    Margin = margin
                }
            };
            var pixelData = qrCodeWriter.Write(JsonConvert.SerializeObject(refundCase));
            byte[] image;
            using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            using (var ms = new MemoryStream())
            {
                var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0,
                        pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                image = ms.ToArray();
            }

            return image;
        }

        private static async Task CreateMerchantAsync(UserManager<RefundeoUser> userManager, RefundeoDbContext context,
            string merchantUsername, string merchantPassword, string merchantCompanyName, string merchantCvrNumber,
            double merchantRefundPercentage, string merchantOpeningHours, string merchantDescription, Address address,
            Location location)
        {
            var user = await CreateAccountAsync(userManager, merchantUsername, merchantPassword, RefundeoConstants.RoleMerchant);
            if (user != null)
            {
                await context.Locations.AddAsync(location);
                await context.Addresses.AddAsync(address);

                await context.SaveChangesAsync();

                await context.MerchantInformations.AddAsync(new MerchantInformation
                {
                    CompanyName = merchantCompanyName,
                    CVRNumber = merchantCvrNumber,
                    RefundPercentage = merchantRefundPercentage,
                    Description = merchantDescription,
                    OpeningHours = merchantOpeningHours,
                    Merchant = user,
                    Location = location,
                    Address = address
                });

                await context.SaveChangesAsync();
            }
        }

        private static async Task<RefundeoUser> CreateAccountAsync(UserManager<RefundeoUser> userManager,
            string username, string password, string role)
        {
            var user = new RefundeoUser {UserName = username};
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

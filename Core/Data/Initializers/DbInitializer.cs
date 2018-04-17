using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Refundeo.Controllers;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.Account;
using Refundeo.Core.Models.QRCode;
using Refundeo.Core.Models.RefundCase;
using ZXing;
using ZXing.QrCode;

namespace Refundeo.Core.Data.Initializers
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(UserManager<RefundeoUser> userManager, RoleManager<IdentityRole> roleManager, RefundeoDbContext context)
        {
            await InitializeRolesAsync(roleManager);
            await InitializeUsersAsync(userManager, context);
            await InitializeRefundCasesAsync(userManager, context);
        }

        private static async Task InitializeRefundCasesAsync(UserManager<RefundeoUser> userManager, RefundeoDbContext context)
        {
            foreach (var refundCase in DbInitializeData.RefundCasesToCreate)
            {
                for (int i = 0; i <= 10; i++)
                {
                    var existingCase = await context.RefundCases
                    .Include(r => r.MerchantInformation)
                    .ThenInclude(m => m.Merchant)
                    .FirstOrDefaultAsync(r => r.MerchantInformation.Merchant.UserName == refundCase.MerchantName && r.Amount == refundCase.Amount);
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
                    await CreateCustomerAsync(userManager, context, user.Username, user.Password, user.Firstname, user.Lastname, user.Country, "123456781234", "1234");
                }
            }
            foreach (var admin in DbInitializeData.AdminsToCreate)
            {
                if (!userManager.Users.Any(u => u.UserName == admin.Username))
                {
                    await CreateAccountAsync(userManager, admin.Username, admin.Password, RefundeoConstants.ROLE_ADMIN);
                }
            }
            foreach (var merchant in DbInitializeData.MerchantsToCreate)
            {
                if (!userManager.Users.Any(u => u.UserName == merchant.Username))
                {
                    await CreateMerchantAsync(userManager, context, merchant.Username, merchant.Password, "MerchantCompany", "12345678", 25);
                }
            }
        }

        private static async Task CreateRefundCaseAsync(RefundeoDbContext context, DbInitializeRefundCase dbInitializeRefundCase)
        {
            var merchantInformation = await context.MerchantInformations.Include(m => m.Merchant).FirstOrDefaultAsync(i => i.Merchant.UserName == dbInitializeRefundCase.MerchantName);
            var customerInformation = await context.CustomerInformations.Include(m => m.Customer).FirstOrDefaultAsync(i => i.Customer.UserName == dbInitializeRefundCase.CustomerName);
            var merchant = merchantInformation?.Merchant;

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

            var refundCaseResult = await context.RefundCases.AddAsync(refundCase);

            await context.SaveChangesAsync();

            var qrCode = new QRCode
            {
                Image = GenerateQRCode(dbInitializeRefundCase.QRCodeHeight, dbInitializeRefundCase.QRCodeWidth, dbInitializeRefundCase.QRCodeMargin, new QRCodePayloadDTO
                {
                    RefundCaseId = refundCase.Id,
                    MerchantId = merchant.Id,
                    RefundAmount = refundCase.Amount
                })
            };
            await context.QRCodes.AddAsync(qrCode);
            refundCase.QRCode = qrCode;
            context.RefundCases.Update(refundCase);
            await context.SaveChangesAsync();
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

        private static byte[] GenerateQRCode(int height, int width, int margin, QRCodePayloadDTO payload)
        {
            var qrCodeWriter = new BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width,
                    Margin = margin
                }
            };
            var pixelData = qrCodeWriter.Write(JsonConvert.SerializeObject(payload));
            byte[] image = null;
            using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
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
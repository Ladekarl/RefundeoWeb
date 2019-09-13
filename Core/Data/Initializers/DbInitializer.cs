using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;

namespace Refundeo.Core.Data.Initializers
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(UserManager<RefundeoUser> userManager,
            RoleManager<IdentityRole> roleManager, RefundeoDbContext context)
        {
            await InitializeTags(context);
            await InitializeRolesAsync(roleManager);
            await InitializeUsersAsync(userManager, context, false);
            await InitializeRefundCasesAsync(context);
        }

        public static async Task InitializeProductionAsync(UserManager<RefundeoUser> userManager,
            RoleManager<IdentityRole> roleManager, RefundeoDbContext context)
        {
            await InitializeTags(context);
            await InitializeRolesAsync(roleManager);
            await InitializeUsersAsync(userManager, context, true);
        }

        private static async Task InitializeTags(RefundeoDbContext context)
        {
            foreach (var tag in DbInitializeData.TagsToCreate)
            {
                if (await context.Tags.Where(t => t.Value == tag.Value).AnyAsync()) continue;
                context.Tags.Add(tag);
            }

            await context.SaveChangesAsync();
        }

        private static async Task InitializeRefundCasesAsync(RefundeoDbContext context)
        {
            foreach (var refundCase in DbInitializeData.RefundCasesToCreate)
            {
                for (var i = 0; i <= 2; i++)
                {
                    var existingCase = await context.RefundCases
                        .Include(r => r.MerchantInformation)
                        .ThenInclude(m => m.Merchants)
                        .FirstOrDefaultAsync(r =>
                            r.MerchantInformation.Merchants.Any(m => m.UserName == refundCase.MerchantName) &&
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

        private static async Task InitializeUsersAsync(UserManager<RefundeoUser> userManager, RefundeoDbContext context,
            bool prod)
        {
            foreach (var user in DbInitializeData.UsersToCreate)
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

            if (prod) return;

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

                    var merchantInformation = new MerchantInformation
                    {
                        CompanyName = merchant.CompanyName,
                        CVRNumber = merchant.CvrNumber,
                        Description = merchant.Description,
                        VATNumber = merchant.VatNumber,
                        VATRate = merchant.VatRate,
                        PriceLevel = merchant.PriceLevel,
                        ContactEmail = merchant.ContactEmail,
                        ContactPhone = merchant.ContactPhone,
                        DateCreated = DateTime.Now,
                        Rating = merchant.Rating,
                        Logo = merchant.Logo,
                        Banner = merchant.Banner,
                        Currency = merchant.Currency
                    };

                    var tags = merchant.Tags.SelectMany(t => context.Tags.Where(tag => tag.Key == t)).ToList();
                    var openingHours = merchant.OpeningHours
                        .Select(o => new OpeningHours {Day = o.Day, Close = o.Close, Open = o.Open}).ToList();

                    var vatPercentage = 100 - 100 / (1 + merchantInformation.VATRate / 100);
                    var feePoints = merchant.FeePoints.Select(f => new FeePoint
                    {
                        AdminFee = f.AdminFee,
                        MerchantFee = f.MerchantFee,
                        End = f.End,
                        Start = f.Start,
                        RefundPercentage = vatPercentage -
                                           vatPercentage * (f.AdminFee / 100) -
                                           vatPercentage * (f.MerchantFee / 100)
                    }).ToList();

                    await CreateMerchantAsync(userManager, context, merchant.Username, merchant.Password,
                        merchantInformation, address, location, openingHours, tags, feePoints);
                }
            }
        }

        private static async Task CreateRefundCaseAsync(RefundeoDbContext context,
            DbInitializeRefundCase dbInitializeRefundCase)
        {
            var merchantInformation = await context.MerchantInformations
                .Include(m => m.Merchants)
                .Where(i => i.Merchants.Any(m => m.UserName == dbInitializeRefundCase.MerchantName))
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
            context.RefundCases.Attach(refundCase);
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
                    DateCreated = DateTime.Now,
                    Customer = user
                });
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateMerchantAsync(UserManager<RefundeoUser> userManager, RefundeoDbContext context,
            string merchantUsername, string merchantPassword, MerchantInformation merchantInformation, Address address, Location location, IList<OpeningHours> openingHours, IEnumerable<Tag> tags,
            IList<FeePoint> feePoints)
        {
            var user = await CreateAccountAsync(userManager, merchantUsername, merchantPassword,
                RefundeoConstants.RoleMerchant);
            if (user != null)
            {
                await context.Locations.AddAsync(location);
                await context.Addresses.AddAsync(address);
                await context.OpeningHours.AddRangeAsync(openingHours);
                await context.FeePoints.AddRangeAsync(feePoints);

                await context.SaveChangesAsync();

                merchantInformation.Location = location;
                merchantInformation.Address = address;

                await context.MerchantInformations.AddAsync(merchantInformation);

                await context.SaveChangesAsync();

                if (context.Entry(merchantInformation).Collection(x => x.Merchants).IsLoaded == false)
                {
                    await context.Entry(merchantInformation).Collection(x => x.Merchants).LoadAsync();
                }

                merchantInformation.Merchants.Add(user);

                if (context.Entry(merchantInformation).Collection(x => x.OpeningHours).IsLoaded == false)
                {
                    await context.Entry(merchantInformation).Collection(x => x.OpeningHours).LoadAsync();
                }

                foreach (var oHours in openingHours)
                {
                    merchantInformation.OpeningHours.Add(oHours);
                }

                if (context.Entry(merchantInformation).Collection(x => x.FeePoints).IsLoaded == false)
                {
                    await context.Entry(merchantInformation).Collection(x => x.FeePoints).LoadAsync();
                }

                foreach (var feePoint in feePoints)
                {
                    merchantInformation.FeePoints.Add(feePoint);
                }

                foreach (var tag in tags)
                {
                    var merchantInformationTag = new MerchantInformationTag
                    {
                        MerchantInformation = merchantInformation,
                        Tag = tag
                    };
                    context.MerchantInformationTags.Add(merchantInformationTag);

                    merchantInformation.MerchantInformationTags.Add(merchantInformationTag);
                    tag.MerchantInformationTags.Add(merchantInformationTag);
                }
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

            return result.Succeeded ? user : null;
        }
    }
}

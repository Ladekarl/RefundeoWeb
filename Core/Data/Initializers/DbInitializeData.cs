using System;
using System.Collections.Generic;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.Account;

namespace Refundeo.Core.Data.Initializers
{
    public static class DbInitializeData
    {
        public static readonly List<string> RolesToCreate = new List<string>
        {
            RefundeoConstants.RoleAdmin,
            RefundeoConstants.RoleMerchant,
            RefundeoConstants.RoleUser
        };

        public static readonly List<UserRegisterDto> UsersTocreate = new List<UserRegisterDto>
        {
            new UserRegisterDto
            {
                Username = "User",
                Password = "User1234!",
                FirstName = "Bob",
                LastName = "Dylan",
                Country = "DK"
            },
            new UserRegisterDto
            {
                Username = "Mike",
                Password = "User1234!",
                FirstName = "Mike",
                LastName = "Tyson",
                Country = "UK"
            }
        };

        public static readonly List<UserRegisterDto> AdminsToCreate = new List<UserRegisterDto>
        {
            new UserRegisterDto
            {
                Username = "Admin",
                Password = "Admin1234!"
            }
        };

        public static readonly List<MerchantRegisterDto> MerchantsToCreate = new List<MerchantRegisterDto>
        {
            new MerchantRegisterDto
            {
                Username = "Merchant",
                Password = "Merchant1234!",
                CompanyName = "MerchantCompany",
                CvrNumber = "12345678",
                RefundPercentage = 25
            },
        };

        public static readonly List<DbInitializeRefundCase> RefundCasesToCreate = new List<DbInitializeRefundCase>
        {
            new DbInitializeRefundCase
            {
                QrCodeHeight = 30,
                QrCodeWidth = 30,
                QrCodeMargin = 0,
                Amount = 150,
                IsRequested = true,
                DateRequested = DateTime.UtcNow,
                MerchantName = "Merchant",
                CustomerName = "Bob"
            },
            new DbInitializeRefundCase
            {
                QrCodeHeight = 30,
                QrCodeWidth = 30,
                QrCodeMargin = 0,
                Amount = 9999,
                IsRequested = true,
                DateRequested = DateTime.UtcNow,
                MerchantName = "Merchant",
                CustomerName = "Mike"
            },
            new DbInitializeRefundCase
            {
                QrCodeHeight = 30,
                QrCodeWidth = 30,
                QrCodeMargin = 0,
                Amount = 3000,
                MerchantName = "Merchant"
            }
        };
    }
}

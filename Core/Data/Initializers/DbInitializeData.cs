using System;
using System.Collections.Generic;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.Account;

namespace Refundeo.Core.Data.Initializers
{
    public static class DbInitializeData
    {
        public static List<string> RolesToCreate = new List<string> {
            RefundeoConstants.ROLE_ADMIN,
            RefundeoConstants.ROLE_MERCHANT,
            RefundeoConstants.ROLE_USER
        };

        public static List<UserRegisterDTO> UsersTocreate = new List<UserRegisterDTO>
        {
            new UserRegisterDTO {
                Username = "User",
                Password = "User1234!",
                Firstname = "Bob",
                Lastname = "Dylan",
                Country = "DK"
            },
            new UserRegisterDTO {
                Username = "Mike",
                Password = "User1234!",
                Firstname = "Mike",
                Lastname = "Tyson",
                Country = "UK"
            }
        };

        public static List<UserRegisterDTO> AdminsToCreate = new List<UserRegisterDTO>
        {
            new UserRegisterDTO {
                Username = "Admin",
                Password = "Admin1234!"
            }
        };

        public static List<MerchantRegisterDTO> MerchantsToCreate = new List<MerchantRegisterDTO>
        {
            new MerchantRegisterDTO {
                Username = "Merchant",
                Password = "Merchant1234!",
                CompanyName = "MerchantCompany",
                CVRNumber = "12345678",
                RefundPercentage = 25
            },
        };

        public static List<DbInitializeRefundCase> RefundCasesToCreate = new List<DbInitializeRefundCase>
        {
            new DbInitializeRefundCase
            {
                QRCodeHeight = 30,
                QRCodeWidth = 30,
                QRCodeMargin = 0,
                Amount = 150,
                IsRequested = true,
                DateRequested = DateTime.UtcNow,
                MerchantName = "Merchant",
                CustomerName = "Bob"
            },
            new DbInitializeRefundCase
            {
                QRCodeHeight = 30,
                QRCodeWidth = 30,
                QRCodeMargin = 0,
                Amount = 9999,
                IsRequested = true,
                DateRequested = DateTime.UtcNow,
                MerchantName = "Merchant",
                CustomerName = "Mike"
            },
            new DbInitializeRefundCase
            {
                QRCodeHeight = 30,
                QRCodeWidth = 30,
                QRCodeMargin = 0,
                Amount = 3000,
                MerchantName = "Merchant"
            }
        };
    }
}
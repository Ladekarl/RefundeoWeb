using System;
using System.Collections.Generic;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.Account;

namespace Refundeo.Core.Data.Initializers
{
    public static class DbInitializeData
    {
        public static readonly List<Tag> TagsToCreate = new List<Tag>
        {
            new Tag
            {
                Key = 0,
                Value = "Jewelry and Watches"
            },
            new Tag
            {
                Key = 1,
                Value = "Clothes"
            },
            new Tag
            {
                Key = 2,
                Value = "Footwear"
            },
            new Tag
            {
                Key = 3,
                Value = "Accessories"
            },
            new Tag
            {
                Key = 4,
                Value = "Sportswear"
            },
            new Tag
            {
                Key = 5,
                Value = "Electronics"
            },
            new Tag
            {
                Key = 6,
                Value = "Children"
            },
            new Tag
            {
                Key = 7,
                Value = "Books and stationary"
            }
        };

        public static readonly List<string> RolesToCreate = new List<string>
        {
            RefundeoConstants.RoleAdmin,
            RefundeoConstants.RoleMerchant,
            RefundeoConstants.RoleUser
        };

        public static readonly List<UserRegisterDto> UsersTocreate = new List<UserRegisterDto>();

        public static readonly List<UserRegisterDto> AdminsToCreate = new List<UserRegisterDto>
        {
            new UserRegisterDto
            {
                Username = "admin@refundeo.com",
                Password = "Playstation4!"
            }
        };

        private static readonly List<OpeningHoursDto> OpeningHoursDtos = new List<OpeningHoursDto>();

        public static readonly List<MerchantRegisterDto> MerchantsToCreate = new List<MerchantRegisterDto>();

        public static readonly List<DbInitializeRefundCase> RefundCasesToCreate = new List<DbInitializeRefundCase>();
    }
}

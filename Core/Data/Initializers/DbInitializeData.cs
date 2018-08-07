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
                Value = "Leather Goods"
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
            },
            new Tag
            {
                Key = 8,
                Value = "Menswear"
            },
            new Tag
            {
                Key = 9,
                Value = "Womenswear"
            }
        };

        public static readonly List<string> RolesToCreate = new List<string>
        {
            RefundeoConstants.RoleAdmin,
            RefundeoConstants.RoleMerchant,
            RefundeoConstants.RoleUser,
            RefundeoConstants.RoleAttachedMerchant
        };

        public static readonly List<UserRegisterDto> UsersTocreate = new List<UserRegisterDto>
        {
            new UserRegisterDto
            {
                Username = "User",
                Password = "User1234!",
                FirstName = "Bob",
                LastName = "Dylan",
                Country = "Denmark"
            },
            new UserRegisterDto
            {
                Username = "Mike",
                Password = "User1234!",
                FirstName = "Mike",
                LastName = "Tyson",
                Country = "England"
            }
        };

        public static readonly List<UserRegisterDto> AdminsToCreate = new List<UserRegisterDto>
        {
            new UserRegisterDto
            {
                Username = "admin@refundeo.com",
                Password = "Playstation4!"
            }
        };

        private static readonly List<OpeningHoursDto> OpeningHoursDtos = new List<OpeningHoursDto>
        {
            new OpeningHoursDto
            {
                Day = 0,
                Open = "08:00",
                Close = "18:00"
            },
            new OpeningHoursDto
            {
                Day = 1,
                Open = "09:00",
                Close = "17:00"
            },
            new OpeningHoursDto
            {
                Day = 2,
                Open = "08:30",
                Close = "16:00"
            },
            new OpeningHoursDto
            {
                Day = 3,
                Open = "08:15",
                Close = "19:00"
            },
            new OpeningHoursDto
            {
                Day = 4,
                Open = "08:00",
                Close = "20:00"
            },
            new OpeningHoursDto
            {
                Day = 5,
                Open = "09:00",
                Close = "21:00"
            },
            new OpeningHoursDto
            {
                Day = 6,
                Open = "10:00",
                Close = "22:00"
            }
        };

        private static readonly List<ChangeFeePointDto> FeePointDtos = new List<ChangeFeePointDto>
        {
            new ChangeFeePointDto
            {
                Start = 0,
                End = 10000,
                MerchantFee = 25,
                AdminFee = 10
            },
            new ChangeFeePointDto
            {
                Start = 10000,
                End = 20000,
                MerchantFee = 10,
                AdminFee = 5
            },
            new ChangeFeePointDto
            {
                Start = 20000,
                MerchantFee = 5,
                AdminFee = 2
            }
        };

        public static readonly List<MerchantRegisterDto> MerchantsToCreate = new List<MerchantRegisterDto>
        {
            new MerchantRegisterDto
            {
                Username = "LouisVuitton",
                Password = "LouisVuitton1234!",
                CompanyName = "Louis Vuitton",
                CvrNumber = "11935893",
                VatRate = 25,
                FeePoints = FeePointDtos,
                AddressCity = "København K",
                AddressCountry = "Danmark",
                AddressPostalCode = "1160",
                AddressStreetName = "Amagertorv",
                Latitude = 55.6785906,
                Longitude = 12.5776793,
                Tags = new List<int> {3},
                AddressStreetNumber = "2",
                OpeningHours = OpeningHoursDtos,
                ContactEmail = "lv@lv.dk",
                ContactPhone = "+45111111111",
                Currency = "GDP",
                VatNumber = "DK 11111111",
                Banner = "https://refundeo20180331121625.blob.core.windows.net/merchantbanners/Louis Vuitton-2-banner",
                Logo = "https://refundeo20180331121625.blob.core.windows.net/merchantlogos/Louis Vuitton-2-logo",
                Description =
                    "Louis Vuitton was a French box-maker and packer who founded the luxury brand of the same name over 150 years ago. From humble beginnings in the French countryside, Vuitton's skill, innovation and determination quickly saw his signature trunks coveted by the world's elite. Now, with Marc Jacobs at the helm as creative director since 1997, the house has expanded its offering to include bags, clothing, shoes, accessories and jewellery, making it one of the most valuable luxury brands in the world."
            },
            new MerchantRegisterDto
            {
                Username = "Klarlund",
                Password = "Klarlund1234!",
                CompanyName = "Klarlund",
                CvrNumber = "75636717",
                VatRate = 25,
                FeePoints = FeePointDtos,
                AddressCity = "København V",
                Currency = "DKK",
                AddressCountry = "Danmark",
                AddressPostalCode = "1620",
                AddressStreetName = "Vesterbrogade",
                AddressStreetNumber = "2 X",
                OpeningHours = OpeningHoursDtos,
                Latitude = 55.674976,
                Tags = new List<int> {0},
                Longitude = 12.564854,
                ContactEmail = "kl@kl.dk",
                ContactPhone = "+4522222222",
                VatNumber = "DK 22222222",
                Banner = "https://refundeo20180331121625.blob.core.windows.net/merchantbanners/Klarlund-3-banner",
                Logo = "https://refundeo20180331121625.blob.core.windows.net/merchantlogos/Klarlund-3-logo",
                Description =
                    "Since 1947, Klarlund has sold exclusive watches and jewellery. The shop on Strøget is located side by side with all the other fashion stores in Copenhagen.At Klarlund you are welcomed by a friendly and smiling staff who make sure that you have a good experience while looking at the wide selection of watches and jewellery.\n\nOn the top floor, the watchmakers are sitting, repairing watches. Besides them there are tables and chairs where you, the customer, takes place if you want to buy a watch or a piece of jewellery.\n\nKlarlund sells brands such as Chanel, Omega, Gucci and Panerai."
            },
            new MerchantRegisterDto
            {
                Username = "Neye",
                Password = "Neye1234!",
                CompanyName = "Neye",
                CvrNumber = "36447125",
                VatRate = 25,
                FeePoints = FeePointDtos,
                AddressCity = "København K",
                AddressCountry = "Danmark",
                Tags = new List<int> {3},
                Currency = "DKK",
                AddressPostalCode = "1157",
                AddressStreetName = "Klosterstræde",
                AddressStreetNumber = "1",
                OpeningHours = OpeningHoursDtos,
                Latitude = 55.678849,
                Longitude = 12.575661,
                ContactEmail = "ne@ne.dk",
                ContactPhone = "+4533333333",
                VatNumber = "DK 33333333",
                Banner = "https://refundeo20180331121625.blob.core.windows.net/merchantbanners/Neye-4-banner",
                Logo = "https://refundeo20180331121625.blob.core.windows.net/merchantlogos/Neye-4-logo",
                Description =
                    "This old Copenhagen firm has a lot of experience when it comes to leather goods.\n\nNeye is close to the traditions of the ancient craft, and at the same time keeps track of developments in design and new types of materials.\n\nIn the flagship store at Strøget you’ll find all you could wish for in leather goods: Bags, brief cases, suitcases, purses, wallets, gloves, belts, etc. and of course all the well-known and popular brands."
            },
            new MerchantRegisterDto
            {
                Username = "Magasin",
                Password = "Magasin1234!",
                CompanyName = "Magasin",
                Currency = "DKK",
                CvrNumber = "58191213",
                VatRate = 25,
                FeePoints = FeePointDtos,
                AddressCity = "København K",
                Tags = new List<int> {8},
                AddressCountry = "Danmark",
                AddressPostalCode = "1050",
                AddressStreetName = "Kongens Nytorv",
                AddressStreetNumber = "13",
                OpeningHours = OpeningHoursDtos,
                Latitude = 55.679224,
                Longitude = 12.58347,
                ContactEmail = "ms@ms.dk",
                ContactPhone = "+4544444444",
                VatNumber = "DK 44444444",
                Banner = "https://refundeo20180331121625.blob.core.windows.net/merchantbanners/Magasin-5-banner",
                Logo = "https://refundeo20180331121625.blob.core.windows.net/merchantlogos/Magasin-5-logo",
                Description =
                    "In Magasin you find an exclusive shopping universe placed in the heart of Aarhus. The department store offers a wide selection of some of the world’s most exclusive and modern brands with a women’s, men’s and children’s department with international brands such as Acne and Kenzo and Danish brands such as By Malene Birger and Stine Goya.\n\nScandinavian Design\n\nThe home department offers a large selection of both classic and modern Scandinavian design as well as a variety of international products. At Magasin you can also treat yourself with high-end beauty and perfume products.\n\nDelicious treats\n\nIf you want something delicious to treat yourself or the beloved ones at home you can find exclusive delicacies with everything from coffee, tea, chocolate, vine or the famous Danish liquorice. If you get hungry or need something tasty you can find both sweet and healthy things in Joe & the juice, Lagkagehuset or the organic eatery Greenilicious.\n\nDiscount to all foreign visitors\n\nIf you are a foreign visitor you get 10% discount on all purchases. Selected departments/brands excluded. All you have to do is show a valid passport, a national identity card or solid proof of a foreign address at the cash register."
            },
            new MerchantRegisterDto
            {
                Username = "Storm",
                Password = "Storm1234!",
                CompanyName = "Storm",
                CvrNumber = "17490486",
                Currency = "DKK",
                VatRate = 25,
                FeePoints = FeePointDtos,
                Tags = new List<int> {0, 1, 2},
                AddressCity = "København",
                AddressCountry = "Danmark",
                AddressPostalCode = "1110",
                AddressStreetName = "Store Regnegade",
                AddressStreetNumber = "1",
                Latitude = 55.681494,
                Longitude = 12.581457,
                OpeningHours = OpeningHoursDtos,
                ContactEmail = "st@st.dk",
                ContactPhone = "+4555555555",
                VatNumber = "DK 55555555",
                Banner = "https://refundeo20180331121625.blob.core.windows.net/merchantbanners/Storm-6-banner",
                Logo = "https://refundeo20180331121625.blob.core.windows.net/merchantlogos/Storm-6-logo",
                Description =
                    "While internationally renowned as one of the leading and most trendsetting lifestyle and fashion stores in Europe, Storm is a lot more than a retail space. With owner Rasmus Storm acting as driver and mediator, it is a conceptual platform where commercial and artistic expressions meet, cultural meanings are exchanged and new concepts evolve. Everyday we strive to evolve and improve the Storm vision to find and share with you our latest and greatest sources of inspiration.\n\nBeyond clothing, we carry a large selection of beauty products, music, magazines, books and always feature exciting and inspirational exhibitions in-store."
            },
            new MerchantRegisterDto
            {
                Username = "Ganni",
                Password = "Ganni1234!",
                CompanyName = "GANNI",
                CvrNumber = "21664731",
                VatRate = 25,
                FeePoints = FeePointDtos,
                Currency = "DKK",
                AddressCity = "København K",
                AddressCountry = "Danmark",
                AddressPostalCode = "1220",
                AddressStreetName = "Frederiksholms Kanal",
                AddressStreetNumber = "4",
                Tags = new List<int> {1, 2, 3},
                OpeningHours = OpeningHoursDtos,
                Latitude = 55.675898,
                Longitude = 12.574912,
                ContactEmail = "gn@gn.dk",
                ContactPhone = "+4566666666",
                VatNumber = "DK 66666666",
                Banner = "https://refundeo20180331121625.blob.core.windows.net/merchantbanners/Ganni-7-banner",
                Logo = "https://refundeo20180331121625.blob.core.windows.net/merchantlogos/Ganni-7-logo",
                Description =
                    "GANNI er et dansk modebrand med hovedkontor i hjertet af København. GANNI er familieejet og har siden 2009 været drevet af Kreativ Direktør Ditte Reffstrup og Direktør Nicolaj Reffstrup. Brandet er repræsenteret i mere end 400 af verdens bedste internationale retailer-butikker samt i 18 egne konceptbutikker i Danmark, Norge og Sverige.\n\nThe GANNI mission is simple: To fill a gap in the advanced contemporary market for effortless, easy-to-wear pieces. From the perfect date-night dress to slouchy-chic Sunday knits, we design to please style-obsessed women everywhere.\n\nGanni har sine rødder i København og byens tilbagelænede og ligefremme kultur. Den skandinaviske arv er essentiel i vores design, dog uden at vi er begrænsede af det traditionelle minimalistiske udtryk. Vi bliver inspireret af modige kvinder i København og verden over."
            }
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
                MerchantName = "Magasin",
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
                MerchantName = "Magasin",
                CustomerName = "Mike"
            },
            new DbInitializeRefundCase
            {
                QrCodeHeight = 30,
                QrCodeWidth = 30,
                QrCodeMargin = 0,
                Amount = 8000,
                DateRequested = DateTime.UtcNow,
                MerchantName = "Magasin",
                CustomerName = "Mike"
            },
            new DbInitializeRefundCase
            {
                QrCodeHeight = 30,
                QrCodeWidth = 30,
                QrCodeMargin = 0,
                Amount = 3000,
                MerchantName = "Magasin"
            }
        };
    }
}

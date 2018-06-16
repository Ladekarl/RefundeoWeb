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
                Username = "LouisVuitton",
                Password = "LouisVuitton1234!",
                CompanyName = "Louis Vuitton",
                CvrNumber = "11935893",
                RefundPercentage = 25,
                AddressCity = "København K",
                AddressCountry = "Danmark",
                AddressPostalCode = "1160",
                AddressStreetName = "Amagertorv",
                Latitude = 55.6785906,
                Longitude = 12.5776793,
                AddressStreetNumber = "2",
                OpeningHours = "10:00 - 18:00",
                Description = "Louis Vuitton was a French box-maker and packer who founded the luxury brand of the same name over 150 years ago. From humble beginnings in the French countryside, Vuitton's skill, innovation and determination quickly saw his signature trunks coveted by the world's elite. Now, with Marc Jacobs at the helm as creative director since 1997, the house has expanded its offering to include bags, clothing, shoes, accessories and jewellery, making it one of the most valuable luxury brands in the world."
            },
            new MerchantRegisterDto
            {
                Username = "Klarlund",
                Password = "Klarlund1234!",
                CompanyName = "Klarlund",
                CvrNumber = "75636717",
                RefundPercentage = 23,
                AddressCity = "København V",
                AddressCountry = "Danmark",
                AddressPostalCode = "1620",
                AddressStreetName = "Vesterbrogade",
                AddressStreetNumber = "2 X",
                OpeningHours = "10:00 - 16:00",
                Latitude = 55.674976,
                Longitude = 12.564854,
                Description = "Since 1947, Klarlund has sold exclusive watches and jewellery. The shop on Strøget is located side by side with all the other fashion stores in Copenhagen.At Klarlund you are welcomed by a friendly and smiling staff who make sure that you have a good experience while looking at the wide selection of watches and jewellery.\n\nOn the top floor, the watchmakers are sitting, repairing watches. Besides them there are tables and chairs where you, the customer, takes place if you want to buy a watch or a piece of jewellery.\n\nKlarlund sells brands such as Chanel, Omega, Gucci and Panerai."
            },
            new MerchantRegisterDto
            {
                Username = "Neye",
                Password = "Neye1234!",
                CompanyName = "Neye",
                CvrNumber = "36447125",
                RefundPercentage = 29,
                AddressCity = "København K",
                AddressCountry = "Danmark",
                AddressPostalCode = "1157",
                AddressStreetName = "Klosterstræde",
                AddressStreetNumber = "1",
                OpeningHours = "10:00 - 18:00",
                Latitude = 55.678849,
                Longitude = 12.575661,
                Description = "This old Copenhagen firm has a lot of experience when it comes to leather goods.\n\nNeye is close to the traditions of the ancient craft, and at the same time keeps track of developments in design and new types of materials.\n\nIn the flagship store at Strøget you’ll find all you could wish for in leather goods: Bags, brief cases, suitcases, purses, wallets, gloves, belts, etc. and of course all the well-known and popular brands."
            },
            new MerchantRegisterDto
            {
                Username = "Magasin",
                Password = "Magasin1234!",
                CompanyName = "Magasin",
                CvrNumber = "58191213",
                RefundPercentage = 25,
                AddressCity = "København K",
                AddressCountry = "Danmark",
                AddressPostalCode = "1050",
                AddressStreetName = "Kongens Nytorv",
                AddressStreetNumber = "13",
                OpeningHours = "10:00 - 22:00",
                Latitude = 55.679224,
                Longitude = 12.58347,
                Description = "In Magasin you find an exclusive shopping universe placed in the heart of Aarhus. The department store offers a wide selection of some of the world’s most exclusive and modern brands with a women’s, men’s and children’s department with international brands such as Acne and Kenzo and Danish brands such as By Malene Birger and Stine Goya.\n\nScandinavian Design\n\nThe home department offers a large selection of both classic and modern Scandinavian design as well as a variety of international products. At Magasin you can also treat yourself with high-end beauty and perfume products.\n\nDelicious treats\n\nIf you want something delicious to treat yourself or the beloved ones at home you can find exclusive delicacies with everything from coffee, tea, chocolate, vine or the famous Danish liquorice. If you get hungry or need something tasty you can find both sweet and healthy things in Joe & the juice, Lagkagehuset or the organic eatery Greenilicious.\n\nDiscount to all foreign visitors\n\nIf you are a foreign visitor you get 10% discount on all purchases. Selected departments/brands excluded. All you have to do is show a valid passport, a national identity card or solid proof of a foreign address at the cash register."
            },
            new MerchantRegisterDto
            {
                Username = "Storm",
                Password = "Storm1234!",
                CompanyName = "Storm",
                CvrNumber = "17490486",
                RefundPercentage = 25,
                AddressCity = "København",
                AddressCountry = "Danmark",
                AddressPostalCode = "1110",
                AddressStreetName = "Store Regnegade",
                AddressStreetNumber = "1",
                Latitude = 55.681494,
                Longitude = 12.581457,
                OpeningHours = "11:00 - 19:00",
                Description = "While internationally renowned as one of the leading and most trendsetting lifestyle and fashion stores in Europe, Storm is a lot more than a retail space. With owner Rasmus Storm acting as driver and mediator, it is a conceptual platform where commercial and artistic expressions meet, cultural meanings are exchanged and new concepts evolve. Everyday we strive to evolve and improve the Storm vision to find and share with you our latest and greatest sources of inspiration.\n\nBeyond clothing, we carry a large selection of beauty products, music, magazines, books and always feature exciting and inspirational exhibitions in-store."
            },
            new MerchantRegisterDto
            {
                Username = "Ganni",
                Password = "Ganni1234!",
                CompanyName = "Ganni",
                CvrNumber = "21664731",
                RefundPercentage = 27,
                AddressCity = "København K",
                AddressCountry = "Danmark",
                AddressPostalCode = "1220",
                AddressStreetName = "Frederiksholms Kanal",
                AddressStreetNumber = "4",
                OpeningHours = "10:00 - 18:00",
                Latitude = 55.675898,
                Longitude = 12.574912,
                Description = "GANNI er et dansk modebrand med hovedkontor i hjertet af København. GANNI er familieejet og har siden 2009 været drevet af Kreativ Direktør Ditte Reffstrup og Direktør Nicolaj Reffstrup. Brandet er repræsenteret i mere end 400 af verdens bedste internationale retailer-butikker samt i 18 egne konceptbutikker i Danmark, Norge og Sverige.\n\nThe GANNI mission is simple: To fill a gap in the advanced contemporary market for effortless, easy-to-wear pieces. From the perfect date-night dress to slouchy-chic Sunday knits, we design to please style-obsessed women everywhere.\n\nGanni har sine rødder i København og byens tilbagelænede og ligefremme kultur. Den skandinaviske arv er essentiel i vores design, dog uden at vi er begrænsede af det traditionelle minimalistiske udtryk. Vi bliver inspireret af modige kvinder i København og verden over."
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

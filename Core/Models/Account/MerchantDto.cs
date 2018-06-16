using System;
using System.Collections.Generic;

namespace Refundeo.Core.Models.Account
{
    public class MerchantDto
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string Id { get; set; }
        public string Username { get; set; }
        public string CompanyName { get; set; }
        public string CvrNumber { get; set; }
        public double RefundPercentage { get; set; }
        public IList<string> Roles { get; set; }
        public string RefreshToken { get; set; }
        public string AddressStreetName { get; set; }
        public string AddressStreetNumber { get; set; }
        public string AddressCity { get; set; }
        public string AddressCountry { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string AddressPostalCode { get; set; }
        public string Description { get; set; }
        public string OpeningHours { get; set; }
        public string Banner { get; set; }
        public string Logo { get; set; }
    }
}

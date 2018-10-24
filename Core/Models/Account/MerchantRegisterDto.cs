using System.Collections.Generic;

namespace Refundeo.Core.Models.Account
{
    public class MerchantRegisterDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public string CvrNumber { get; set; }
        public double VatRate { get; set; }
        public double Rating { get; set; }
        public int PriceLevel { get; set; }
        public string AddressStreetName { get; set; }
        public string AddressStreetNumber { get; set; }
        public string AddressCity { get; set; }
        public string AddressCountry { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string AddressPostalCode { get; set; }
        public string Description { get; set; }
        public string Banner { get; set; }
        public string Logo { get; set; }
        public string VatNumber { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string AdminEmail { get; set; }
        public string Currency { get; set; }
        public CityDto City { get; set; }
        public IList<ChangeFeePointDto> FeePoints { get; set; }
        public IList<OpeningHoursDto> OpeningHours { get; set; }
        public IList<int> Tags { get; set; }
    }
}

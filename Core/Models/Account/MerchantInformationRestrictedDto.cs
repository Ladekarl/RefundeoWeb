using System;
using System.Collections.Generic;

namespace Refundeo.Core.Models.Account
{
    public class MerchantInformationRestrictedDto
    {
        public string CompanyName { get; set; }
        public string CvrNumber { get; set; }
        public double VatRate { get; set; }
        public string AddressStreetName { get; set; }
        public string AddressStreetNumber { get; set; }
        public string AddressCity { get; set; }
        public string AddressCountry { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string AddressPostalCode { get; set; }
        public string Description { get; set; }
        public string Banner { get; set; }
        public string Logo { get; set; }
        public string VatNumber { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string Currency { get; set; }
        public double MinRefundPercentage { get; set; }
        public DateTime DateCreated { get; set; }
        public ICollection<OpeningHoursDto> OpeningHours { get; set; }
        public ICollection<int> Tags { get; set; }
    }
}

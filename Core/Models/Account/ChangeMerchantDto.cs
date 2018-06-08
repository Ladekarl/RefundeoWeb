namespace Refundeo.Core.Models.Account
{
    public class ChangeMerchantDto
    {
        public string Username { get; set; }
        public string CompanyName { get; set; }
        public string CvrNumber { get; set; }
        public double RefundPercentage { get; set; }
        public string AddressStreetName { get; set; }
        public string AddressStreetNumber { get; set; }
        public string AddressCity { get; set; }
        public string AddressCountry { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}

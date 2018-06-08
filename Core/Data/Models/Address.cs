namespace Refundeo.Core.Data.Models
{
    public class Address
    {
        public long Id { get; set; }
        public string StreetName { get; set; }
        public string StreetNumber { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}

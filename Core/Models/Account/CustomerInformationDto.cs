namespace Refundeo.Core.Models.Account
{
    public class CustomerInformationDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public bool IsOauth { get; set; }
        public bool AcceptedPrivacyPolicy { get; set; }
        public bool AcceptedTermsOfService { get; set; }
        public string PrivacyPolicy { get; set; }
        public string TermsOfService { get; set; }
        public string Swift { get; set; }
        public string Passport { get; set; }
        public string AddressStreetName { get; set; }
        public string AddressStreetNumber { get; set; }
        public string AddressCity { get; set; }
        public string AddressCountry { get; set; }
        public string AddressPostalCode { get; set; }
        public string QRCode { get; set; }
    }
}

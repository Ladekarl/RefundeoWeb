using System;
using System.Collections.Generic;

namespace Refundeo.Core.Models.Account
{
    public class CustomerDto
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
        public string Swift { get; set; }
        public string Passport { get; set; }
        public bool IsOauth {get; set;}
        public bool AcceptedTermsOfService { get; set; }
        public bool AcceptedPrivacyPolicy { get; set; }
        public string PrivacyPolicy { get; set; }
        public string TermsOfService { get; set; }
        public IList<string> Roles { get; set; }
        public string RefreshToken { get; set; }
        public string AddressStreetName { get; set; }
        public string AddressStreetNumber { get; set; }
        public string AddressCity { get; set; }
        public string AddressCountry { get; set; }
    }
}

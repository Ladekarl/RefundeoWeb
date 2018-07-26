using System;
using System.Collections.Generic;

namespace Refundeo.Core.Data.Models
{
    // TODO: Secure these columns to support GDPR
    public class CustomerInformation
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
        public string AccountNumber { get; set; }
        public string Swift { get; set; }
        public string Passport { get; set; }
        public bool IsOauth { get; set; }
        public bool AcceptedPrivacyPolicy { get; set; }
        public bool AcceptedTermsOfService { get; set; }
        public string PrivacyPolicy { get; set; }
        public string TermsOfService { get; set; }
        public int TermsOfServiceVersion { get; set; }
        public int PrivacyPolicyVersion { get; set; }
        public string QRCode { get; set; }
        public string CustomerId { get; set; }
        public virtual RefundeoUser Customer { get; set; }
        public virtual Address Address { get; set; }
        public DateTime DateCreated { get; set; }
        public ICollection<RefundCase> RefundCases { get; set; }
        public string Language { get; set; }
    }
}

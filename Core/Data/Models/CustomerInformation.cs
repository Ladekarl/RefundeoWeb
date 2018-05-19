using System.Collections.Generic;

namespace Refundeo.Core.Data.Models
{
    // TODO: Secure these columns to support GDPR
    public class CustomerInformation
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankRegNumber { get; set; }
        public bool AcceptedPrivacyPolicy { get; set; }
        public string PrivacyPolicy { get; set; }
        public virtual RefundeoUser Customer { get; set; }
        public ICollection<RefundCase> RefundCases { get; set; }
    }
}

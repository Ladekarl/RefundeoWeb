using System.Collections.Generic;

namespace Refundeo.Data.Models
{
    public class CustomerInformation
    {
        public long Id {get; set;}
        public string FirstName {get; set;}
        public string LastName {get; set;}
        public string Country {get; set;}
        // TODO: Secure these columns
        public string BankAccountNumber {get; set;}
        public string BankRegNumber {get; set;}
        public virtual RefundeoUser Customer { get; set; }
        public ICollection<RefundCase> RefundCases { get; set; }
    }
}
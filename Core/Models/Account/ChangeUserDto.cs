using System.Collections.Generic;

namespace Refundeo.Core.Models.Account
{
    public class ChangeUserDto
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankRegNumber { get; set; }
        public bool AcceptedPrivacyPolicy { get; set; }
        public string PrivacyPolicy { get; set; }
    }
}

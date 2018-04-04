using System.Collections.Generic;

namespace Refundeo.Models.Account
{
    public class AccountRegisterDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public ICollection<string> Roles { get; set; }
    }
}
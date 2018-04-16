using System.Collections.Generic;

namespace Refundeo.Core.Models.Account
{
    public class ChangeUserDTO
    {
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Country { get; set; }
    }
}
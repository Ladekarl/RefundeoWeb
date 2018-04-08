using System.Collections.Generic;

namespace Refundeo.Models.Account
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public IList<string> Roles { get; set; }
    }
}
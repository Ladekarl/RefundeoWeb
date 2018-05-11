using System.Collections.Generic;

namespace Refundeo.Core.Models.Account
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public IList<string> Roles { get; set; }
    }
}

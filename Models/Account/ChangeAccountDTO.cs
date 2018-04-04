using System.Collections.Generic;

namespace Refundeo.Models.Account
{
    public class ChangeAccountDTO
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public ICollection<string> Roles { get; set; }
    }
}
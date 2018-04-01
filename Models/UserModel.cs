using System.Collections.Generic;

namespace Refundeo.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public IList<string> Roles { get; set;}
    }
}
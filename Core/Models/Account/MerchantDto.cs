using System;
using System.Collections.Generic;

namespace Refundeo.Core.Models.Account
{
    public class MerchantDto
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string Id { get; set; }
        public string Username { get; set; }
        public string CompanyName { get; set; }
        public string CvrNumber { get; set; }
        public int RefundPercentage { get; set; }
        public IList<string> Roles { get; set; }
        public string RefreshToken { get; set; }
    }
}

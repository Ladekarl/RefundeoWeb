using System.Collections.Generic;

namespace Refundeo.Core.Models.Account
{
    public class ChangeMerchantDto
    {
        public string Username { get; set; }
        public string CompanyName { get; set; }
        public string CvrNumber { get; set; }
        public int RefundPercentage { get; set; }
    }
}

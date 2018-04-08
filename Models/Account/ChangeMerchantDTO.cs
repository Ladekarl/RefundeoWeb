using System.Collections.Generic;

namespace Refundeo.Models.Account
{
    public class ChangeMerchantDTO
    {
        public string Username { get; set; }
        public string CompanyName { get; set; }
        public string CVRNumber { get; set; }
        public int RefundPercentage { get; set; }
    }
}
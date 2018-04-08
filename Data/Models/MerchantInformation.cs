using System.Collections.Generic;

namespace Refundeo.Data.Models
{
    public class MerchantInformation
    {
        public long Id {get; set;}
        public string CompanyName { get; set; }
        public string CVRNumber { get; set; }
        public int RefundPercentage { get; set; }
        public virtual RefundeoUser Merchant { get; set; }
        public ICollection<RefundCase> RefundCases { get; set; }
    }
}
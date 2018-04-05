using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Refundeo.Data.Models
{
    public class RefundeoUser : IdentityUser
    {
        public ICollection<RefundCase> MerchantRefundCases {get; set;}
        public ICollection<RefundCase> CustomerRefundCases { get; set; }
    }
}
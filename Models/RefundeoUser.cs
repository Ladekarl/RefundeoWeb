using Microsoft.AspNetCore.Identity;

namespace Refundeo.Models
{
    public class RefundeoUser : IdentityUser
    {
        public bool IsMerchant { get; set; }
    }
}
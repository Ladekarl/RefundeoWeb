using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Refundeo.Core.Data.Models
{
    public class RefundeoUser : IdentityUser
    {
        public virtual string RefreshToken { get; set; }
        public virtual CustomerInformation CustomerInformation { get; set; }
        public virtual MerchantInformation MerchantInformation { get; set; }
    }
}

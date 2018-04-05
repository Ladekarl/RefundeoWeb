using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Refundeo.Data.Models
{
    public class RefundeoUser : IdentityUser
    {
        public ICollection<QRCode> qrCodes {get; set;}
    }
}
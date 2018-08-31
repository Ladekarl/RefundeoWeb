using System;
using System.Collections.Generic;

namespace Refundeo.Core.Data.Models
{
    // TODO: Possibly secure these columns too
    public class MerchantInformation
    {
        public long Id { get; set; }
        public double VATRate { get; set; }
        public string CompanyName { get; set; }
        public string CVRNumber { get; set; }
        public string Description { get; set; }
        public string Banner { get; set; }
        public string Logo { get; set; }
        public string VATNumber { get; set; }
        public string ContactEmail { get; set; }
        public string AdminEmail { get; set; }
        public string ContactPhone { get; set; }
        public string Currency { get; set; }
        public DateTime DateCreated { get; set; }
        public virtual Location Location { get; set; }
        public virtual Address Address { get; set; }
        public virtual City City { get; set; }
        public virtual ICollection<RefundeoUser> Merchants { get; set; }
        public virtual ICollection<MerchantInformationTag> MerchantInformationTags { get; set; }
        public virtual ICollection<FeePoint> FeePoints { get; set; }
        public virtual ICollection<RefundCase> RefundCases { get; set; }
        public virtual ICollection<OpeningHours> OpeningHours { get; set; }
    }
}

using System.Collections.Generic;

namespace Refundeo.Core.Data.Models
{
    public class Tag
    {
        public long Id { get; set; }
        public int Key { get; set; }
        public string Value { get; set; }
        public virtual ICollection<MerchantInformationTag> MerchantInformationTags { get; set; }
    }
}

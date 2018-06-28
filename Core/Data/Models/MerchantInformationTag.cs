namespace Refundeo.Core.Data.Models
{
    public class MerchantInformationTag
    {
        public long MerhantInformationId { get; set; }
        public MerchantInformation MerchantInformation { get; set; }
        public long TagId { get; set; }
        public Tag Tag { get; set; }
    }
}

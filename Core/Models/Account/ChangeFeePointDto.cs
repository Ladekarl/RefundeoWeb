namespace Refundeo.Core.Models.Account
{
    public class ChangeFeePointDto
    {
        public double MerchantFee { get; set; }
        public double AdminFee { get; set; }
        public double Start { get; set; }
        public double? End { get; set; }
    }
}

namespace Refundeo.Core.Models.Account
{
    public class FeePointDto
    {
        public double MerchantFee { get; set; }
        public double AdminFee { get; set; }
        public double RefundPercentage { get; set; }
        public double Start { get; set; }
        public double? End { get; set; }
    }
}

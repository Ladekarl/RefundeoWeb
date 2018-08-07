namespace Refundeo.Core.Data.Models
{
    public class FeePoint
    {
        public long Id { get; set; }
        public double MerchantFee { get; set; }
        public double AdminFee { get; set; }
        public double RefundPercentage { get; set; }
        public double Start { get; set; }
        public double? End { get; set; }
    }
}

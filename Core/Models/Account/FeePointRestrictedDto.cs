namespace Refundeo.Core.Models.Account
{
    public class FeePointRestrictedDto
    {
        public double RefundPercentage { get; set; }
        public double Start { get; set; }
        public double? End { get; set; }
    }
}

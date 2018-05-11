namespace Refundeo.Core.Models.RefundCase
{
    public class AdminCreateRefundCaseDto
    {
        public int QrCodeHeight { get; set; }
        public int QrCodeWidth { get; set; }
        public int QrCodeMargin { get; set; }
        public double Amount { get; set; }
        public string MerchantId { get; set; }
        public string CustomerId { get; set; }
    }
}

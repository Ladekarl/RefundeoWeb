namespace Refundeo.Models.RefundCase
{
    public class AdminCreateRefundCaseDTO
    {
        public int QRCodeHeight { get; set; }
        public int QRCodeWidth { get; set; }
        public int QRCodeMargin { get; set; }
        public double Amount { get; set; }
        public string MerchantId { get; set; }
        public string CustomerId { get; set; }
    }
}
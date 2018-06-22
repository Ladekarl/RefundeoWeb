namespace Refundeo.Core.Models.RefundCase
{
    public class CreateRefundCaseDto
    {
        public double Amount { get; set; }
        public int QrCodeHeight { get; set; }
        public int QrCodeWidth { get; set; }
        public int QrCodeMargin { get; set; }
        public string ReceiptNumber { get; set; }
        public string CustomerId { get; set; }
        public string MerchantSignature { get; set; }
        public string CustomerSignature { get; set; }
    }
}

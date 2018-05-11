namespace Refundeo.Core.Models.RefundCase
{
    public class CreateRefundCaseDto
    {
        public double Amount { get; set; }
        public int QrCodeHeight { get; set; }
        public int QrCodeWidth { get; set; }
        public int QrCodeMargin { get; set; }
    }
}

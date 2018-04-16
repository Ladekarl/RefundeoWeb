namespace Refundeo.Core.Models.RefundCase
{
    public class CreateRefundCaseDTO
    {
        public double Amount { get; set; }
        public int QRCodeHeight { get; set; }
        public int QRCodeWidth { get; set; }
        public int QRCodeMargin { get; set; }
    }
}
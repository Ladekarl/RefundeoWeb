namespace Refundeo.Models.RefundCase
{
    public class RefundCaseDTO
    {
        public long Id { get; set; }
        public double Amount { get; set; }
        public double RefundAmount { get; set; }
        public string QRCode { get; set; }
        public string Documentation { get; set; }
    }
}
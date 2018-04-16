namespace Refundeo.Core.Models.RefundCase
{
    public class AdminUpdateRefundCaseDTO
    {
        public long Id { get; set; }
        public double Amount { get; set; }
        public double RefundAmount { get; set; }
        public bool IsRequested { get; set; }
        public bool IsAccepted { get; set; }
        public string QRCode { get; set; }
        public string Documentation { get; set; }
        public string MerchantId { get; set; }
        public string CustomerId { get; set; }
    }
}
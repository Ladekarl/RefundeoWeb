namespace Refundeo.Core.Models.RefundCase
{
    public class AdminUpdateRefundCaseDto
    {
        public double Amount { get; set; }
        public double RefundAmount { get; set; }
        public bool IsRequested { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsRejected { get; set; }
        public string QrCode { get; set; }
        public string Documentation { get; set; }
        public string MerchantId { get; set; }
        public string CustomerId { get; set; }
        public string ReceiptNumber { get; set; }
    }
}

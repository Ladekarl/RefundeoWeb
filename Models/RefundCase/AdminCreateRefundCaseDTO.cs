namespace Refundeo.Models.RefundCase
{
    public class AdminCreateRefundCaseDTO
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int Margin { get; set; }
        public double Amount { get; set; }
        public string MerchantId { get; set; }
        public string CustomerId { get; set; }
    }
}
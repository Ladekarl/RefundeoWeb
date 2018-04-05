namespace Refundeo.Data.Models
{
    public class QRCode
    {
        public long Id { get; set; }
        public double Amount { get; set; }
        public double RefundAmount { get; set; }
        public string Image { get; set; }
        public RefundeoUser Merchant { get; set; }
    }
}
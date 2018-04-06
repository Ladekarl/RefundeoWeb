namespace Refundeo.Data.Models
{
    public class RefundCase
    {
        public long Id { get; set; }
        public double Amount { get; set; }
        public double RefundAmount { get; set; }
        public bool IsRequested { get; set; }
        public string CustomerId { get; set; }
        public virtual RefundeoUser Customer { get; set; }
        public string MerchantId { get; set; }
        public RefundeoUser Merchant { get; set; }
        public virtual QRCode QRCode { get; set; }
        public virtual Documentation Documentation { get; set; }
    }
}
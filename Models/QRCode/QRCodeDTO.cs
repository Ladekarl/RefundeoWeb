namespace Refundeo.Models.QRCode
{
    public class QRCodeDTO
    {
        public long Id {get; set;}
        public double Amount { get; set; }
        public double RefundAmount { get; set; }
        public string Image { get; set; }
    }
}
namespace Refundeo.Core.Models.QRCode
{
    public class QRCodePayloadDTO
    {
        public long RefundCaseId { get; set; }
        public string MerchantId { get; set; }
        public double RefundAmount { get; set; }
    }
}
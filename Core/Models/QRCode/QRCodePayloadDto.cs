namespace Refundeo.Core.Models.QRCode
{
    public class QRCodePayloadDto
    {
        public long RefundCaseId { get; set; }
        public string MerchantId { get; set; }
        public double RefundAmount { get; set; }
    }
}

namespace Refundeo.Core.Models.RefundCase
{
    public class VatFormModel
    {
        public string MerchantName { get; set; }
        public string MerchantAddres { get; set; }
        public string MerchantPostalCode { get; set; }
        public string MerchantCity { get; set; }
        public string MerchantCountry { get; set; }
        public string MerchantVatNo { get; set; }
        public string MerchantPhone { get; set; }
        public string MerchantEmail { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddres { get; set; }
        public string CustomerPostalCode { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerCountry { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPassport { get; set; }
        public string ReceiptNumber { get; set; }
        public string Date { get; set; }
        public string Amount { get; set; }
        public string RefundAmount { get; set; }
        public string VatAmount { get; set; }
        public string MerchantSignature { get; set; }
        public string CustomerSignature { get; set; }
        public string QrCode { get; set; }
    }
}

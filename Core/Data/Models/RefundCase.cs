using System;

namespace Refundeo.Core.Data.Models
{
    public class RefundCase
    {
        public long Id { get; set; }
        public double Amount { get; set; }
        public double RefundAmount { get; set; }
        public double AdminAmount { get; set; }
        public double MerchantAmount { get; set; }
        public double VATAmount { get; set; }
        public string ReceiptNumber { get; set; }
        public string MerchantSignature { get; set; }
        public string CustomerSignature { get; set; }
        public string VATFormImage { get; set; }
        public bool IsRequested { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsRejected { get; set; }
        public virtual string QRCode { get; set; }
        public virtual string ReceiptImage { get; set; }
        public DateTime DateCreated { get; set; }
        public virtual DateTime DateRequested { get; set; }
        public virtual CustomerInformation CustomerInformation { get; set; }
        public MerchantInformation MerchantInformation { get; set; }
    }
}

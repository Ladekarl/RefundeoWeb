using System;

namespace Refundeo.Core.Data.Initializers
{
    public class DbInitializeRefundCase
    {
        public int QrCodeHeight { get; set; }
        public int QrCodeWidth { get; set; }
        public int QrCodeMargin { get; set; }
        public double Amount { get; set; }
        public string MerchantName { get; set; }
        public string CustomerName { get; set; }
        public DateTime DateRequested { get; set; }
        public bool IsRequested { get; set; }
    }
}

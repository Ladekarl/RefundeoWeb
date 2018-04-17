using System;

namespace Refundeo.Core.Data.Initializers
{
    public class DbInitializeRefundCase
    {
        public int QRCodeHeight { get; set; }
        public int QRCodeWidth { get; set; }
        public int QRCodeMargin { get; set; }
        public double Amount { get; set; }
        public string MerchantName { get; set; }
        public string CustomerName { get; set; }
        public DateTime DateRequested { get; set; }
        public bool IsRequested { get; set; }
    }
}
using System;
using Refundeo.Core.Models.Account;

namespace Refundeo.Core.Models.RefundCase
{
        public class RefundCaseAdminDto
    {
        public long Id { get; set; }
        public double Amount { get; set; }
        public double RefundAmount { get; set; }
        public double AdminAmount { get; set; }
        public double MerchantAmount { get; set; }
        public double VatAmount { get; set; }
        public bool IsRequested { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsRejected { get; set; }
        public string VatFormImage { get; set; }
        public string ReceiptImage { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateRequested { get; set; }
        public MerchantInformationDto Merchant { get; set; }
        public CustomerInformationDto Customer { get; set; }
        public string ReceiptNumber { get; set; }
    }
}

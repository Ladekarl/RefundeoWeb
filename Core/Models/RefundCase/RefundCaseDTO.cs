using System;
using Refundeo.Core.Models.Account;

namespace Refundeo.Core.Models.RefundCase
{
    public class RefundCaseDTO
    {
        public long Id { get; set; }
        public double Amount { get; set; }
        public double RefundAmount { get; set; }
        public bool IsRequested { get; set; }
        public bool IsAccepted { get; set; }
        public string QRCode { get; set; }
        public string Documentation { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateRequested { get; set; }
        public MerchantInformationDTO Merchant { get; set; }
        public CustomerInformationDTO Customer { get; set; }
    }
}
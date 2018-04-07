using Refundeo.Models.Account;

namespace Refundeo.Models.RefundCase
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
        public UserDTO Merchant { get; set; }
        public UserDTO Customer { get; set; }
    }
}
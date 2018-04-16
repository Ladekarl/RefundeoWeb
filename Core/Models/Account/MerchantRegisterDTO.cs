namespace Refundeo.Core.Models.Account
{
    public class MerchantRegisterDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public string CVRNumber { get; set; }
        public int RefundPercentage { get; set; }
    }
}
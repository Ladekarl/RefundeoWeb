namespace Refundeo.Core.Models.Account
{
    public class MerchantRegisterDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public string CvrNumber { get; set; }
        public int RefundPercentage { get; set; }
    }
}

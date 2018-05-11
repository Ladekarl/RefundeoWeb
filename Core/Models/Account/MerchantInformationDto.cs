namespace Refundeo.Core.Models.Account
{
    public class MerchantInformationDto
    {
        public string Id { get; set; }
        public string CompanyName { get; set; }
        public string CvrNumber { get; set; }
        public int RefundPercentage { get; set; }
    }
}

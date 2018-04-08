namespace Refundeo.Models.Account
{
    public class MerchantInformationDTO
    {
        public string Id { get; set; }
        public string CompanyName { get; set; }
        public string CVRNumber { get; set; }
        public int RefundPercentage { get; set; }
    }
}
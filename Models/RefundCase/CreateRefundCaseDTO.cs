namespace Refundeo.Models.RefundCase
{
    public class CreateRefundCaseDTO
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int Margin { get; set; }
        public double Amount { get; set; }
    }
}
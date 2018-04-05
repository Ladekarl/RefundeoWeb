namespace Refundeo.Models.QRCode
{
    public class CreateQRCodeDTO
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int Margin { get; set; }
        public double Amount { get; set; }
    }
}
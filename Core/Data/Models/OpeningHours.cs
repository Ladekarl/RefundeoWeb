namespace Refundeo.Core.Data.Models
{
    public class OpeningHours
    {
        public long Id { get; set; }
        public int Day { get; set; }
        public string Open { get; set; }
        public string Close { get; set; }
    }
}

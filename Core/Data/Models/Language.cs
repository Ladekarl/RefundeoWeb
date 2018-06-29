namespace Refundeo.Core.Data.Models
{
    public class Language
    {
        public long Id { get; set; }
        public string Key { get; set; }
        public string RefundUpdateTitle { get; set; }
        public string RefundUpdateText { get; set; }
        public string RefundCreatedTitle { get; set; }
        public string RefundCreatedText { get; set; }
    }
}

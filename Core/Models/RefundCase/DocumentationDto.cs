using Microsoft.AspNetCore.Http;

namespace Refundeo.Core.Models.RefundCase
{
    public class DocementationDto
    {
        public long RefundCaseId { get; set; }
        public IFormFile File { get; set; }
    }
}
